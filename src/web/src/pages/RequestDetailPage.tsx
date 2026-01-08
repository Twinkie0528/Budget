import { useState, useEffect } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { api, BudgetRequestDetail, ApiError } from '../api/client'

function RequestDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { token } = useAuth()
  
  const [request, setRequest] = useState<BudgetRequestDetail | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isExporting, setIsExporting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (id) {
      loadRequest(id)
    }
  }, [id, token])

  async function loadRequest(requestId: string) {
    setIsLoading(true)
    setError(null)
    try {
      const data = await api.getRequest(requestId, token)
      setRequest(data)
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.detail || err.message)
      } else {
        setError('Failed to load request')
      }
    } finally {
      setIsLoading(false)
    }
  }

  async function handleExport() {
    if (!id) return
    
    setIsExporting(true)
    try {
      const blob = await api.exportRequest(id, token)
      
      // Create download link
      const url = window.URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `BudgetRequest_${request?.requestNumber || id}.xlsx`
      document.body.appendChild(a)
      a.click()
      window.URL.revokeObjectURL(url)
      document.body.removeChild(a)
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.detail || 'Export failed')
      } else {
        setError('Export failed')
      }
    } finally {
      setIsExporting(false)
    }
  }

  if (isLoading) {
    return (
      <div className="loading">
        <div className="spinner" />
        <p>Loading request...</p>
      </div>
    )
  }

  if (error && !request) {
    return (
      <div className="card" style={{ borderColor: 'var(--color-danger)' }}>
        <p style={{ color: 'var(--color-danger)' }}>{error}</p>
        <Link to="/requests" className="btn btn-secondary mt-16">
          ← Back to Requests
        </Link>
      </div>
    )
  }

  if (!request) {
    return null
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-16">
        <div>
          <Link to="/requests" style={{ fontSize: '13px', color: 'var(--color-text-secondary)' }}>
            ← Back to Requests
          </Link>
          <h2 className="page-title" style={{ marginTop: '8px', marginBottom: '4px' }}>
            {request.requestNumber}
          </h2>
          <p style={{ color: 'var(--color-text-secondary)' }}>{request.title}</p>
        </div>
        <div className="flex gap-8">
          <button 
            className="btn btn-secondary"
            onClick={handleExport}
            disabled={isExporting}
          >
            {isExporting ? 'Exporting...' : '📥 Export Excel'}
          </button>
        </div>
      </div>

      {error && (
        <div className="card mb-16" style={{ borderColor: 'var(--color-danger)' }}>
          <p style={{ color: 'var(--color-danger)' }}>{error}</p>
        </div>
      )}

      {/* Summary Card */}
      <div className="card">
        <div className="flex items-center justify-between mb-16">
          <div className="card-title" style={{ margin: 0 }}>Request Details</div>
          <span className={`badge badge-${getStatusBadge(request.status)}`}>
            {request.status}
          </span>
        </div>
        
        <div className="detail-grid">
          <div className="detail-item">
            <label>Request Number</label>
            <span>{request.requestNumber}</span>
          </div>
          <div className="detail-item">
            <label>Title</label>
            <span>{request.title}</span>
          </div>
          <div className="detail-item">
            <label>Channel</label>
            <span>{request.channel || '-'}</span>
          </div>
          <div className="detail-item">
            <label>Owner</label>
            <span>{request.owner || '-'}</span>
          </div>
          <div className="detail-item">
            <label>Frequency</label>
            <span>{request.frequency || '-'}</span>
          </div>
          <div className="detail-item">
            <label>Vendor</label>
            <span>{request.vendor || '-'}</span>
          </div>
          <div className="detail-item">
            <label>Fiscal Year</label>
            <span>{request.fiscalYear}{request.fiscalQuarter ? ` Q${request.fiscalQuarter}` : ''}</span>
          </div>
          <div className="detail-item">
            <label>Total Amount</label>
            <span style={{ fontWeight: 600, fontSize: '16px' }}>
              {formatCurrency(request.totalAmount, request.currency)}
            </span>
          </div>
          {request.description && (
            <div className="detail-item" style={{ gridColumn: '1 / -1' }}>
              <label>Description</label>
              <span>{request.description}</span>
            </div>
          )}
        </div>

        <div style={{ marginTop: '16px', paddingTop: '16px', borderTop: '1px solid var(--color-border)' }}>
          <div className="detail-grid" style={{ fontSize: '12px' }}>
            <div className="detail-item">
              <label>Created</label>
              <span>{formatDateTime(request.createdAt)} by {request.createdBy}</span>
            </div>
            {request.updatedAt && (
              <div className="detail-item">
                <label>Updated</label>
                <span>{formatDateTime(request.updatedAt)} by {request.updatedBy}</span>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Items Table */}
      <div className="card">
        <div className="card-title">
          Line Items
          <span style={{ fontWeight: 'normal', color: 'var(--color-text-secondary)', marginLeft: '8px' }}>
            ({request.items.length} rows)
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
                <th>Account</th>
              </tr>
            </thead>
            <tbody>
              {request.items.length === 0 ? (
                <tr>
                  <td colSpan={9} className="text-center" style={{ color: 'var(--color-text-secondary)' }}>
                    No line items
                  </td>
                </tr>
              ) : (
                request.items.map((item) => (
                  <tr key={item.id}>
                    <td>{item.rowNumber}</td>
                    <td>{item.lineDescription || '-'}</td>
                    <td>{item.category || '-'}</td>
                    <td>{item.subCategory || '-'}</td>
                    <td className="text-right">{item.quantity?.toFixed(2) || '-'}</td>
                    <td className="text-right">{item.unitPrice?.toFixed(2) || '-'}</td>
                    <td className="text-right">{item.amount.toFixed(2)}</td>
                    <td>{item.costCenter || '-'}</td>
                    <td>{item.accountCode || '-'}</td>
                  </tr>
                ))
              )}
            </tbody>
            {request.items.length > 0 && (
              <tfoot>
                <tr style={{ fontWeight: 600 }}>
                  <td colSpan={6} className="text-right">Total:</td>
                  <td className="text-right">
                    {request.items.reduce((sum, item) => sum + item.amount, 0).toFixed(2)}
                  </td>
                  <td colSpan={2}></td>
                </tr>
              </tfoot>
            )}
          </table>
        </div>
      </div>
    </div>
  )
}

function formatCurrency(amount: number, currency: string): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: currency || 'USD'
  }).format(amount)
}

function formatDateTime(dateStr: string): string {
  return new Date(dateStr).toLocaleString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

function getStatusBadge(status: string): string {
  switch (status) {
    case 'Draft': return 'draft'
    case 'Pending': return 'pending'
    case 'Approved': return 'approved'
    case 'Rejected': return 'rejected'
    default: return 'draft'
  }
}

export default RequestDetailPage

