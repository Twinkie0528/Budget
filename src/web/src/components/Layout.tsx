import { Outlet, NavLink } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

function Layout() {
  const { user, isInTeams, isLoading } = useAuth()

  if (isLoading) {
    return (
      <div className="app">
        <div className="loading">
          <div className="spinner" />
          <p>Loading...</p>
        </div>
      </div>
    )
  }

  return (
    <div className="app">
      <header className="header">
        <h1>Budget Platform</h1>
        <nav className="header-nav">
          <NavLink to="/teams" className={({ isActive }) => isActive ? 'active' : ''}>
            Home
          </NavLink>
          <NavLink to="/imports" className={({ isActive }) => isActive ? 'active' : ''}>
            Import
          </NavLink>
          <NavLink to="/requests" className={({ isActive }) => isActive ? 'active' : ''}>
            Requests
          </NavLink>
        </nav>
        <div className="header-user">
          {user ? (
            <span>{user.name} {isInTeams && '(Teams)'}</span>
          ) : (
            <span>Not signed in</span>
          )}
        </div>
      </header>
      <main className="main">
        <Outlet />
      </main>
    </div>
  )
}

export default Layout

