import { createContext, useContext, useEffect, useState, ReactNode } from 'react'
import * as microsoftTeams from '@microsoft/teams-js'

interface User {
  id: string
  name: string
  email?: string
}

interface AuthContextType {
  user: User | null
  token: string | null
  isLoading: boolean
  isInTeams: boolean
  error: string | null
}

const AuthContext = createContext<AuthContextType>({
  user: null,
  token: null,
  isLoading: true,
  isInTeams: false,
  error: null
})

export function useAuth() {
  return useContext(AuthContext)
}

interface AuthProviderProps {
  children: ReactNode
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<User | null>(null)
  const [token, setToken] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isInTeams, setIsInTeams] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    initializeAuth()
  }, [])

  async function initializeAuth() {
    try {
      // Check if we're running inside Teams
      const inTeams = await checkIfInTeams()
      setIsInTeams(inTeams)

      if (inTeams) {
        await initializeTeamsAuth()
      } else {
        initializeDevAuth()
      }
    } catch (err) {
      console.error('Auth initialization failed:', err)
      setError(err instanceof Error ? err.message : 'Authentication failed')
      // Fall back to dev auth on error
      initializeDevAuth()
    } finally {
      setIsLoading(false)
    }
  }

  async function checkIfInTeams(): Promise<boolean> {
    try {
      // Check URL params or iframe context
      const urlParams = new URLSearchParams(window.location.search)
      if (urlParams.get('inTeams') === 'true') {
        return true
      }

      // Try to initialize Teams SDK
      await microsoftTeams.app.initialize()
      const context = await microsoftTeams.app.getContext()
      return !!context.app?.host?.name
    } catch {
      return false
    }
  }

  async function initializeTeamsAuth() {
    try {
      await microsoftTeams.app.initialize()
      
      // Get Teams context for user info
      const context = await microsoftTeams.app.getContext()
      
      // Get auth token
      const authToken = await microsoftTeams.authentication.getAuthToken()
      setToken(authToken)

      // Set user from context
      setUser({
        id: context.user?.id || 'teams-user',
        name: context.user?.displayName || 'Teams User',
        email: context.user?.userPrincipalName
      })
    } catch (err) {
      console.error('Teams auth failed:', err)
      throw err
    }
  }

  function initializeDevAuth() {
    // Development mode: use mock user
    const devMode = import.meta.env.VITE_DEV_AUTH === 'true' || 
                    import.meta.env.MODE === 'development'
    
    if (devMode) {
      setUser({
        id: 'dev-user-001',
        name: 'Local Developer',
        email: 'developer@localhost'
      })
      setToken('dev-token')
    }
  }

  return (
    <AuthContext.Provider value={{ user, token, isLoading, isInTeams, error }}>
      {children}
    </AuthContext.Provider>
  )
}

