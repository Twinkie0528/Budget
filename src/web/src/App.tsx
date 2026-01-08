import { Routes, Route, Navigate } from 'react-router-dom'
import Layout from './components/Layout'
import TeamsPage from './pages/TeamsPage'
import ImportsPage from './pages/ImportsPage'
import ImportPreviewPage from './pages/ImportPreviewPage'
import RequestsPage from './pages/RequestsPage'
import RequestDetailPage from './pages/RequestDetailPage'

function App() {
  return (
    <Routes>
      <Route path="/" element={<Layout />}>
        <Route index element={<Navigate to="/teams" replace />} />
        <Route path="teams" element={<TeamsPage />} />
        <Route path="imports" element={<ImportsPage />} />
        <Route path="imports/:id/preview" element={<ImportPreviewPage />} />
        <Route path="requests" element={<RequestsPage />} />
        <Route path="requests/:id" element={<RequestDetailPage />} />
      </Route>
    </Routes>
  )
}

export default App

