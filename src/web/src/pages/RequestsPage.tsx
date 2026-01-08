import { useState, useEffect, FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { api, BudgetRequestList, PagedResult, RequestFilters, BudgetRequestStatus, ApiError } from '../api/client'

function RequestsPage() {
  const { token } = useAuth()
  
  const [data, setData] = useState<PagedResult<BudgetRequestList> | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  
  // Filters
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState<BudgetRequestStatus | ''>('')
  const [fiscalYear, setFiscalYear] = useState<number | ''>('')
  const [page, setPage] = useState(1)
  const pageSize = 20

  useEffect(() => {
    loadRequests()
  }, [page, token])

  async function loadRequests(resetPage = false) {
    setIsLoading(true)
    setError(null)
    
    const currentPage = resetPage ? 1 : page
    if (resetPage) setPage(1)

    const filters: RequestFilters = {
      page: currentPage,
      pageSize,
      search: search || undefined,
      status: status || undefined,
      fiscalYear: fiscalYear || undefined
    }

    try {
      const result = await api.getRequests(filters, token)
      setData(result)
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.detail || err.message)
      } else {
        setError('Failed to load requests')
      }
    } finally {
      setIsLoading(false)
    }
  }

  function handleSearch(e: FormEvent) {
    e.preventDefault()
    loadRequests(true)
  }

  function handleClearFilters() {
    setSearch('')
    setStatus('')
    setFiscalYear('')
    setPage(1)
    // Trigger reload after state updates
    setTimeout(() => loadRequests(true), 0)
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-16">
        <h2 className="page-title" style={{ marginBottom: 0 }}>Budget Requests</h2>
        <Link to="/imports" className="btn btn-primary">
          + New Import
        </Link>
      </div>

      {/* Filters */}
      <form onSubmit={handleSearch} className="card">
        <div className="filters">
          <input
            type="text"
            className="form-input"
            placeholder="Search..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            style={{ minWidth: '200px' }}
          />
          <select
            className="form-input"
            value={status}
            onChange={(e) => setStatus(e.target.value as BudgetRequestStatus | '')}
          >
            <option value="">All Status</option>
            <option value="Draft">Draft</option>
            <option value="Pending">Pending</option>
            <option value="Approved">Approved</option>
            <option value="Rejected">Rejected</option>
            <option value="Archived">Archived</option>
          </select>
          <input
            type="number"
            className="form-input"
            placeholder="Fiscal Year"
            value={fiscalYear}
            onChange={(e) => setFiscalYear(e.target.value ? parseInt(e.target.value) : '')}
            style={{ width: '120px' }}
          />
          <button type="submit" className="btn btn-primary">
            Search
          </button>
          <button type="button" className="btn btn-secondary" onClick={handleClearFilters}>
            Clear
          </button>
        </div>
      </form>

      {error && (
        <div className="card" style={{ borderColor: 'var(--color-danger)' }}>
          <p style={{ color: 'var(--color-danger)' }}>{error}</p>
        </div>
      )}

      {/* Results Table */}
      <div className="card">
        {isLoading ? (
          <div className="loading">
            <div className="spinner" />
          </div>
        ) : data && data.items.length > 0 ? (
          <>
            <div className="table-container">
              <table>
                <thead>
                  <tr>
                    <th>Request #</th>
                    <th>Title</th>
                    <th>Channel</th>
                    <th>Owner</th>
                    <th className="text-right">Amount</th>
                    <th>Status</th>
                    <th>FY</th>
                    <th>Items</th>
                    <th>Created</th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((req) => (
                    <tr key={req.id}>
                      <td>
                        <Link to={`/requests/${req.id}`}>{req.requestNumber}</Link>
                      </td>
                      <td style={{ maxWidth: '200px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                        {req.title}
                      </td>
                      <td>{req.channel || '-'}</td>
                      <td>{req.owner || '-'}</td>
                      <td className="text-right">
                        {formatCurrency(req.totalAmount, req.currency)}
                      </td>
                      <td>
                        <span className={`badge badge-${getStatusBadge(req.status)}`}>
                          {req.status}
                        </span>
                      </td>
                      <td>{req.fiscalYear}</td>
                      <td className="text-center">{req.itemCount}</td>
                      <td style={{ whiteSpace: 'nowrap' }}>
                        {formatDate(req.createdAt)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Pagination */}
            {data.totalPages > 1 && (
              <div className="flex items-center justify-between mt-16">
                <span style={{ color: 'var(--color-text-secondary)', fontSize: '13px' }}>
                  Showing {(page - 1) * pageSize + 1} - {Math.min(page * pageSize, data.totalCount)} of {data.totalCount}
                </span>
                <div className="flex gap-8">
                  <button
                    className="btn btn-secondary btn-sm"
                    disabled={page === 1}
                    onClick={() => setPage(p => p - 1)}
                  >
                    Previous
                  </button>
                  <span style={{ padding: '4px 8px' }}>
                    Page {page} of {data.totalPages}
                  </span>
                  <button
                    className="btn btn-secondary btn-sm"
                    disabled={page >= data.totalPages}
                    onClick={() => setPage(p => p + 1)}
                  >
                    Next
                  </button>
                </div>
              </div>
            )}
          </>
        ) : (
          <div className="text-center" style={{ padding: '40px', color: 'var(--color-text-secondary)' }}>
            <p>No budget requests found</p>
            <Link to="/imports" className="btn btn-primary mt-16">
              Import Your First Budget
            </Link>
          </div>
        )}
      </div>
    </div>
  )
}

function formatCurrency(amount: number, currency: string): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: currency || 'USD',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0
  }).format(amount)
}

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

function getStatusBadge(status: BudgetRequestStatus): string {
  switch (status) {
    case 'Draft': return 'draft'
    case 'Pending': return 'pending'
    case 'Approved': return 'approved'
    case 'Rejected': return 'rejected'
    default: return 'draft'
  }
}

export default RequestsPage

