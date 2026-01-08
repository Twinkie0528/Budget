const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api'

interface RequestOptions {
  method?: string
  body?: unknown
  token?: string | null
}

class ApiError extends Error {
  status: number
  detail?: string

  constructor(message: string, status: number, detail?: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.detail = detail
  }
}

async function request<T>(endpoint: string, options: RequestOptions = {}): Promise<T> {
  const { method = 'GET', body, token } = options

  const headers: HeadersInit = {}
  
  if (body && !(body instanceof FormData)) {
    headers['Content-Type'] = 'application/json'
  }
  
  if (token) {
    headers['Authorization'] = `Bearer ${token}`
  }

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    method,
    headers,
    body: body instanceof FormData ? body : body ? JSON.stringify(body) : undefined
  })

  if (!response.ok) {
    let errorDetail: string | undefined
    try {
      const errorData = await response.json()
      errorDetail = errorData.detail || errorData.title
    } catch {
      // Ignore JSON parse errors
    }
    throw new ApiError(
      errorDetail || `Request failed: ${response.statusText}`,
      response.status,
      errorDetail
    )
  }

  // Handle empty responses
  const contentType = response.headers.get('content-type')
  if (!contentType || !contentType.includes('application/json')) {
    return response as unknown as T
  }

  return response.json()
}

// Types
export interface UploadResult {
  importRunId: string
  fileName: string
  fileSizeBytes: number
  status: ImportStatus
}

export type ImportStatus = 
  | 'Uploaded' 
  | 'Parsing' 
  | 'Parsed' 
  | 'ParseFailed' 
  | 'Committing' 
  | 'Committed' 
  | 'CommitFailed'

export interface ImportPreview {
  importRunId: string
  fileName: string
  status: ImportStatus
  header: ParsedHeader | null
  items: ParsedItem[]
  errors: ValidationError[]
  canCommit: boolean
}

export interface ParsedHeader {
  requestNumber?: string
  title?: string
  description?: string
  channel?: string
  owner?: string
  frequency?: string
  vendor?: string
  totalAmount?: number
  currency?: string
  fiscalYear?: number
  fiscalQuarter?: number
}

export interface ParsedItem {
  rowNumber: number
  lineDescription?: string
  category?: string
  subCategory?: string
  amount?: number
  quantity?: number
  unitPrice?: number
  costCenter?: string
  accountCode?: string
  hasErrors: boolean
}

export interface ValidationError {
  rowNumber?: number
  field: string
  message: string
  severity: 'Error' | 'Warning'
}

export interface CommitResult {
  importRunId: string
  budgetRequestId: string
  requestNumber: string
  itemCount: number
}

export interface BudgetRequestList {
  id: string
  requestNumber: string
  title: string
  channel?: string
  owner?: string
  totalAmount: number
  currency: string
  status: BudgetRequestStatus
  fiscalYear: number
  fiscalQuarter?: number
  createdAt: string
  createdBy: string
  itemCount: number
}

export type BudgetRequestStatus = 'Draft' | 'Pending' | 'Approved' | 'Rejected' | 'Archived'

export interface BudgetRequestDetail {
  id: string
  requestNumber: string
  title: string
  description?: string
  channel?: string
  owner?: string
  frequency?: string
  vendor?: string
  totalAmount: number
  currency: string
  status: BudgetRequestStatus
  fiscalYear: number
  fiscalQuarter?: number
  fiscalMonth?: number
  createdAt: string
  createdBy: string
  updatedAt?: string
  updatedBy?: string
  importRunId?: string
  items: BudgetItem[]
}

export interface BudgetItem {
  id: string
  rowNumber: number
  lineDescription?: string
  category?: string
  subCategory?: string
  amount: number
  quantity?: number
  unitPrice?: number
  costCenter?: string
  accountCode?: string
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface RequestFilters {
  page?: number
  pageSize?: number
  search?: string
  status?: BudgetRequestStatus
  channel?: string
  owner?: string
  fiscalYear?: number
}

// API Functions
export const api = {
  // Imports
  async uploadFile(file: File, token?: string | null): Promise<UploadResult> {
    const formData = new FormData()
    formData.append('file', file)
    return request<UploadResult>('/imports/upload', {
      method: 'POST',
      body: formData,
      token
    })
  },

  async getImportPreview(id: string, token?: string | null): Promise<ImportPreview> {
    return request<ImportPreview>(`/imports/${id}/preview`, { token })
  },

  async commitImport(id: string, token?: string | null): Promise<CommitResult> {
    return request<CommitResult>(`/imports/${id}/commit`, {
      method: 'POST',
      token
    })
  },

  // Requests
  async getRequests(filters: RequestFilters = {}, token?: string | null): Promise<PagedResult<BudgetRequestList>> {
    const params = new URLSearchParams()
    if (filters.page) params.set('page', filters.page.toString())
    if (filters.pageSize) params.set('pageSize', filters.pageSize.toString())
    if (filters.search) params.set('search', filters.search)
    if (filters.status) params.set('status', filters.status)
    if (filters.channel) params.set('channel', filters.channel)
    if (filters.owner) params.set('owner', filters.owner)
    if (filters.fiscalYear) params.set('fiscalYear', filters.fiscalYear.toString())
    
    const query = params.toString()
    return request<PagedResult<BudgetRequestList>>(`/requests${query ? `?${query}` : ''}`, { token })
  },

  async getRequest(id: string, token?: string | null): Promise<BudgetRequestDetail> {
    return request<BudgetRequestDetail>(`/requests/${id}`, { token })
  },

  async exportRequest(id: string, token?: string | null): Promise<Blob> {
    const response = await fetch(`${API_BASE_URL}/requests/${id}/export`, {
      headers: token ? { 'Authorization': `Bearer ${token}` } : {}
    })
    if (!response.ok) {
      throw new ApiError('Export failed', response.status)
    }
    return response.blob()
  }
}

export { ApiError }

