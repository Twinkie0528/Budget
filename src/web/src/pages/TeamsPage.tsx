import { Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

function TeamsPage() {
  const { user, isInTeams, error } = useAuth()

  return (
    <div>
      <h2 className="page-title">Welcome to Budget Platform</h2>
      
      {error && (
        <div className="card" style={{ borderColor: 'var(--color-danger)' }}>
          <p style={{ color: 'var(--color-danger)' }}>Auth Error: {error}</p>
        </div>
      )}

      <div className="card">
        <div className="card-title">Current User</div>
        {user ? (
          <div className="detail-grid">
            <div className="detail-item">
              <label>Name</label>
              <span>{user.name}</span>
            </div>
            <div className="detail-item">
              <label>User ID</label>
              <span>{user.id}</span>
            </div>
            {user.email && (
              <div className="detail-item">
                <label>Email</label>
                <span>{user.email}</span>
              </div>
            )}
            <div className="detail-item">
              <label>Environment</label>
              <span className={`badge ${isInTeams ? 'badge-approved' : 'badge-draft'}`}>
                {isInTeams ? 'Microsoft Teams' : 'Development'}
              </span>
            </div>
          </div>
        ) : (
          <p>Not authenticated</p>
        )}
      </div>

      <div className="card">
        <div className="card-title">Quick Actions</div>
        <div className="flex gap-8" style={{ marginTop: '12px' }}>
          <Link to="/imports" className="btn btn-primary">
            Upload Budget File
          </Link>
          <Link to="/requests" className="btn btn-secondary">
            View Requests
          </Link>
        </div>
      </div>

      <div className="card">
        <div className="card-title">About</div>
        <p style={{ color: 'var(--color-text-secondary)' }}>
          Budget Platform enables you to upload Excel budget files, review parsed data, 
          and manage budget requests. This tab is optimized for Microsoft Teams integration.
        </p>
      </div>
    </div>
  )
}

export default TeamsPage

