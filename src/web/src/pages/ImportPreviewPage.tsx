import { useState, useEffect } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { api, ImportPreview, ApiError } from '../api/client'

function ImportPreviewPage() {
  const { id } = useParams<{ id: string }>()
  const { token } = useAuth()
  const navigate = useNavigate()
  
  const [preview, setPreview] = useState<ImportPreview | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isCommitting, setIsCommitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (id) {
      loadPreview(id)
    }
  }, [id, token])

  async function loadPreview(importId: string) {
    setIsLoading(true)
    setError(null)
    try {
      const data = await api.getImportPreview(importId, token)
      setPreview(data)
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.detail || err.message)
      } else {
        setError('Failed to load preview')
      }
    } finally {
      setIsLoading(false)
    }
  }

  async function handleCommit() {
    if (!id || !preview?.canCommit) return
    
    setIsCommitting(true)
    setError(null)
    
    try {
      const result = await api.commitImport(id, token)
      navigate(`/requests/${result.budgetRequestId}`)
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.detail || err.message)
      } else {
        setError('Failed to commit import')
      }
      setIsCommitting(false)
    }
  }

  if (isLoading) {
    return (
      <div className="loading">
        <div className="spinner" />
        <p>Loading preview...</p>
      </div>
    )
  }

  if (error && !preview) {
    return (
      <div className="card" style={{ borderColor: 'var(--color-danger)' }}>
        <p style={{ color: 'var(--color-danger)' }}>{error}</p>
        <Link to="/imports" className="btn btn-secondary mt-16">
          ← Back to Import
        </Link>
      </div>
    )
  }

  if (!preview) {
    return null
  }

  const errorCount = preview.errors.filter(e => e.severity === 'Error').length
  const warningCount = preview.errors.filter(e => e.severity === 'Warning').length

  return (
    <div>
      <div className="flex items-center justify-between mb-16">
        <div>
          <h2 className="page-title" style={{ marginBottom: '4px' }}>Import Preview</h2>
          <p style={{ color: 'var(--color-text-secondary)' }}>{preview.fileName}</p>
        </div>
        <div className="flex gap-8">
          <Link to="/imports" className="btn btn-secondary">
            ← Cancel
          </Link>
          <button 
            className="btn btn-success"
            onClick={handleCommit}
            disabled={!preview.canCommit || isCommitting}
          >
            {isCommitting ? 'Committing...' : 'Commit Import'}
          </button>
        </div>
      </div>

      {error && (
        <div className="card mb-16" style={{ borderColor: 'var(--color-danger)' }}>
          <p style={{ color: 'var(--color-danger)' }}>{error}</p>
        </div>
      )}

      {/* Validation Summary */}
      {preview.errors.length > 0 && (
        <div className="card">
          <div className="card-title">
            Validation Results
            {errorCount > 0 && (
              <span className="badge badge-error" style={{ marginLeft: '8px' }}>
                {errorCount} error{errorCount > 1 ? 's' : ''}
              </span>
            )}
            {warningCount > 0 && (
              <span className="badge badge-pending" style={{ marginLeft: '8px' }}>
                {warningCount} warning{warningCount > 1 ? 's' : ''}
              </span>
            )}
          </div>
          <ul className="error-list">
            {preview.errors.map((err, idx) => (
              <li key={idx} className={`error-item ${err.severity.toLowerCase()}`}>
                {err.rowNumber ? `Row ${err.rowNumber}: ` : ''}
                <strong>{err.field}</strong> - {err.message}
              </li>
            ))}
          </ul>
          {errorCount > 0 && (
            <p style={{ marginTop: '12px', color: 'var(--color-danger)', fontSize: '13px' }}>
              ⚠️ Cannot commit import with errors. Please fix the source file and re-upload.
            </p>
          )}
        </div>
      )}

      {/* Header Info */}
      {preview.header && (
        <div className="card">
          <div className="card-title">Header Information</div>
          <div className="detail-grid">
            {preview.header.title && (
              <div className="detail-item">
                <label>Title</label>
                <span>{preview.header.title}</span>
              </div>
            )}
            {preview.header.requestNumber && (
              <div className="detail-item">
                <label>Request Number</label>
                <span>{preview.header.requestNumber}</span>
              </div>
            )}
            {preview.header.channel && (
              <div className="detail-item">
                <label>Channel</label>
                <span>{preview.header.channel}</span>
              </div>
            )}
            {preview.header.owner && (
              <div className="detail-item">
                <label>Owner</label>
                <span>{preview.header.owner}</span>
              </div>
            )}
            {preview.header.fiscalYear && (
              <div className="detail-item">
                <label>Fiscal Year</label>
                <span>{preview.header.fiscalYear}</span>
              </div>
            )}
            {preview.header.currency && (
              <div className="detail-item">
                <label>Currency</label>
                <span>{preview.header.currency}</span>
              </div>
            )}
            {preview.header.totalAmount !== undefined && (
              <div className="detail-item">
                <label>Total Amount</label>
                <span>{formatCurrency(preview.header.totalAmount, preview.header.currency)}</span>
              </div>
            )}
            {preview.header.description && (
              <div className="detail-item" style={{ gridColumn: '1 / -1' }}>
                <label>Description</label>
                <span>{preview.header.description}</span>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Items Table */}
      <div className="card">
        <div className="card-title">
          Line Items
          <span style={{ fontWeight: 'normal', color: 'var(--color-text-secondary)', marginLeft: '8px' }}>
            ({preview.items.length} rows)
          </span>
        </div>
        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>#</th>
                <th>Description</th>
                <th>Category</th>
                <th>Sub-Category</th>
                <th className="text-right">Qty</th>
                <th className="text-right">Unit Price</th>
                <th className="text-right">Amount</th>
                <th>Cost Center</th>
              </tr>
            </thead>
            <tbody>
              {preview.items.length === 0 ? (
                <tr>
                  <td colSpan={8} className="text-center" style={{ color: 'var(--color-text-secondary)' }}>
                    No line items found
                  </td>
                </tr>
              ) : (
                preview.items.map((item) => (
                  <tr key={item.rowNumber} style={item.hasErrors ? { background: '#fde7e9' } : undefined}>
                    <td>{item.rowNumber}</td>
                    <td>{item.lineDescription || '-'}</td>
                    <td>{item.category || '-'}</td>
                    <td>{item.subCategory || '-'}</td>
                    <td className="text-right">{item.quantity?.toFixed(2) || '-'}</td>
                    <td className="text-right">{item.unitPrice?.toFixed(2) || '-'}</td>
                    <td className="text-right">{item.amount?.toFixed(2) || '-'}</td>
                    <td>{item.costCenter || '-'}</td>
                  </tr>
                ))
              )}
            </tbody>
            {preview.items.length > 0 && (
              <tfoot>
                <tr style={{ fontWeight: 600 }}>
                  <td colSpan={6} className="text-right">Total:</td>
                  <td className="text-right">
                    {preview.items.reduce((sum, item) => sum + (item.amount || 0), 0).toFixed(2)}
                  </td>
                  <td></td>
                </tr>
              </tfoot>
            )}
          </table>
        </div>
      </div>
    </div>
  )
}

function formatCurrency(amount: number, currency?: string): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: currency || 'USD'
  }).format(amount)
}

export default ImportPreviewPage

