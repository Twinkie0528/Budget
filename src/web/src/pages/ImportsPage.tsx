import { useState, useRef, DragEvent, ChangeEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { api, UploadResult, ApiError } from '../api/client'

function ImportsPage() {
  const { token } = useAuth()
  const navigate = useNavigate()
  const fileInputRef = useRef<HTMLInputElement>(null)
  
  const [isDragOver, setIsDragOver] = useState(false)
  const [isUploading, setIsUploading] = useState(false)
  const [uploadResult, setUploadResult] = useState<UploadResult | null>(null)
  const [error, setError] = useState<string | null>(null)

  const handleDragOver = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault()
    setIsDragOver(true)
  }

  const handleDragLeave = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault()
    setIsDragOver(false)
  }

  const handleDrop = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault()
    setIsDragOver(false)
    
    const files = e.dataTransfer.files
    if (files.length > 0) {
      handleFile(files[0])
    }
  }

  const handleFileSelect = (e: ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files
    if (files && files.length > 0) {
      handleFile(files[0])
    }
  }

  const handleFile = async (file: File) => {
    // Validate file type
    const validTypes = [
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      'application/vnd.ms-excel'
    ]
    const validExtensions = ['.xlsx', '.xls']
    
    const hasValidType = validTypes.includes(file.type)
    const hasValidExtension = validExtensions.some(ext => 
      file.name.toLowerCase().endsWith(ext)
    )

    if (!hasValidType && !hasValidExtension) {
      setError('Please upload an Excel file (.xlsx or .xls)')
      return
    }

    setError(null)
    setIsUploading(true)
    setUploadResult(null)

    try {
      const result = await api.uploadFile(file, token)
      setUploadResult(result)
      
      // Navigate to preview if parsed successfully
      if (result.status === 'Parsed') {
        navigate(`/imports/${result.importRunId}/preview`)
      }
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.detail || err.message)
      } else {
        setError('Upload failed. Please try again.')
      }
    } finally {
      setIsUploading(false)
    }
  }

  const handleZoneClick = () => {
    fileInputRef.current?.click()
  }

  return (
    <div>
      <h2 className="page-title">Import Budget File</h2>
      <p className="page-subtitle">Upload an Excel file to create a new budget request</p>

      <div className="card">
        <div
          className={`upload-zone ${isDragOver ? 'drag-over' : ''}`}
          onClick={handleZoneClick}
          onDragOver={handleDragOver}
          onDragLeave={handleDragLeave}
          onDrop={handleDrop}
        >
          <input
            ref={fileInputRef}
            type="file"
            accept=".xlsx,.xls"
            onChange={handleFileSelect}
          />
          <div className="upload-icon">📁</div>
          {isUploading ? (
            <>
              <div className="spinner mb-8" />
              <p>Uploading and parsing...</p>
            </>
          ) : (
            <>
              <p><strong>Drop your Excel file here</strong></p>
              <p style={{ color: 'var(--color-text-secondary)', marginTop: '8px' }}>
                or click to browse
              </p>
              <p style={{ color: 'var(--color-text-secondary)', fontSize: '12px', marginTop: '16px' }}>
                Supported formats: .xlsx, .xls
              </p>
            </>
          )}
        </div>
      </div>

      {error && (
        <div className="card" style={{ borderColor: 'var(--color-danger)' }}>
          <p style={{ color: 'var(--color-danger)' }}>{error}</p>
        </div>
      )}

      {uploadResult && (
        <div className="card">
          <div className="card-title">Upload Result</div>
          <div className="detail-grid">
            <div className="detail-item">
              <label>File Name</label>
              <span>{uploadResult.fileName}</span>
            </div>
            <div className="detail-item">
              <label>Size</label>
              <span>{formatFileSize(uploadResult.fileSizeBytes)}</span>
            </div>
            <div className="detail-item">
              <label>Status</label>
              <span className={`badge badge-${getStatusBadge(uploadResult.status)}`}>
                {uploadResult.status}
              </span>
            </div>
          </div>
          
          {uploadResult.status === 'Parsed' && (
            <div className="mt-16">
              <button 
                className="btn btn-primary"
                onClick={() => navigate(`/imports/${uploadResult.importRunId}/preview`)}
              >
                View Preview →
              </button>
            </div>
          )}
          
          {uploadResult.status === 'ParseFailed' && (
            <div className="mt-16">
              <p style={{ color: 'var(--color-danger)' }}>
                Failed to parse the file. Please check the format and try again.
              </p>
            </div>
          )}
        </div>
      )}

      <div className="card">
        <div className="card-title">Template Requirements</div>
        <ul style={{ paddingLeft: '20px', color: 'var(--color-text-secondary)' }}>
          <li>Header information in cells B2-B11 (Title, Request Number, etc.)</li>
          <li>Detail rows starting at row 14</li>
          <li>Columns: Description, Category, Sub-Category, Quantity, Unit Price, Amount</li>
          <li>Monthly breakdown columns (Jan-Dec) optional</li>
        </ul>
      </div>
    </div>
  )
}

function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`
}

function getStatusBadge(status: string): string {
  switch (status) {
    case 'Parsed':
    case 'Committed':
      return 'parsed'
    case 'ParseFailed':
    case 'CommitFailed':
      return 'error'
    default:
      return 'pending'
  }
}

export default ImportsPage

