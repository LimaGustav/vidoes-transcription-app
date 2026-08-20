import { useState } from 'react'
import './App.css'

const API_BASE_URL = 'http://localhost:5034/api' // ajuste pra porta real da tua API

function App() {
  const [token, setToken] = useState('')
  const [file, setFile] = useState(null)
  const [title, setTitle] = useState('')
  const [status, setStatus] = useState('idle') // idle | requesting | uploading | confirming | done | error
  const [log, setLog] = useState([])
  const [uploadProgress, setUploadProgress] = useState(0)

  const addLog = (message) => {
    setLog((prev) => [...prev, `${new Date().toLocaleTimeString()} - ${message}`])
  }

  const handleFileChange = (e) => {
    const selected = e.target.files[0]
    setFile(selected)
    if (selected) addLog(`Arquivo selecionado: ${selected.name} (${selected.type}, ${(selected.size / 1024 / 1024).toFixed(2)} MB)`)
  }

  // 1. Pede a URL presignada pra API
  const requestUpload = async () => {
    const response = await fetch(`${API_BASE_URL}/videos/upload`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`
      },
      body: JSON.stringify({
        title,
        contentType: file.type,
        categoryId: null
      })
    })

    if (!response.ok) {
      const errorText = await response.text()
      throw new Error(`Falha ao solicitar upload (${response.status}): ${errorText}`)
    }

    return response.json() // { videoId, uploadUrl, expiresAt }
  }

  // 2. Sobe o arquivo direto no MinIO, com progresso via XMLHttpRequest
  const uploadToMinio = (uploadUrl) => {
    return new Promise((resolve, reject) => {
      const xhr = new XMLHttpRequest()
      xhr.open('PUT', uploadUrl)
      xhr.setRequestHeader('Content-Type', file.type)

      xhr.upload.onprogress = (event) => {
        if (event.lengthComputable) {
          const percent = Math.round((event.loaded / event.total) * 100)
          setUploadProgress(percent)
        }
      }

      xhr.onload = () => {
        if (xhr.status >= 200 && xhr.status < 300) {
          resolve()
        } else {
          reject(new Error(`Falha no upload pro MinIO (${xhr.status}): ${xhr.responseText}`))
        }
      }

      xhr.onerror = () => reject(new Error('Erro de rede durante o upload pro MinIO'))

      xhr.send(file)
    })
  }

  // 3. Confirma o upload na API
  // const confirmUpload = async (videoId) => {
  //   const response = await fetch(`${API_BASE_URL}/videos/${videoId}/confirm`, {
  //     method: 'POST',
  //     headers: {
  //       Authorization: `Bearer ${token}`
  //     }
  //   })

  //   if (!response.ok) {
  //     const errorText = await response.text()
  //     throw new Error(`Falha ao confirmar upload (${response.status}): ${errorText}`)
  //   }

  //   return response.json() // { videoId, status }
  // }

  // Orquestra o fluxo completo
  const handleUploadFlow = async () => {
    if (!file) return addLog('Escolha um arquivo primeiro.')
    if (!title.trim()) return addLog('Preencha um título.')
    if (!token.trim()) return addLog('Cole um token JWT primeiro.')

    try {
      setStatus('requesting')
      setUploadProgress(0)
      addLog('Solicitando URL de upload à API...')
      const { videoId, uploadUrl } = await requestUpload()
      addLog(`URL recebida. VideoId: ${videoId}`)

      setStatus('uploading')
      addLog('Enviando arquivo direto pro MinIO...')
      await uploadToMinio(uploadUrl)
      addLog('Upload pro MinIO concluído.')

      setStatus('done')
    } catch (err) {
      setStatus('error')
      addLog(`Erro: ${err.message}`)
    }
  }

  return (
    <div style={{ maxWidth: 600, margin: '2rem auto', fontFamily: 'sans-serif' }}>

      <div style={{ marginBottom: 12 }}>
        <label>Token JWT</label>
        <input
          type="text"
          value={token}
          onChange={(e) => setToken(e.target.value)}
          placeholder="Cole o token aqui"
          style={{ width: '100%', padding: 8 }}
        />
      </div>

      <div style={{ marginBottom: 12 }}>
        <label>Título do vídeo</label>
        <input
          type="text"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="Ex: Aula 1 - Introdução"
          style={{ width: '100%', padding: 8 }}
        />
      </div>

      <div style={{ marginBottom: 12 }}>
        <input type="file" accept="video/*" onChange={handleFileChange} />
      </div>

      <button
        onClick={handleUploadFlow}
        disabled={status === 'requesting' || status === 'uploading' || status === 'confirming'}
      >
        Enviar vídeo
      </button>

      {status === 'uploading' && (
        <div style={{ marginTop: 12 }}>
          <progress value={uploadProgress} max="100" style={{ width: '100%' }} />
          <span>{uploadProgress}%</span>
        </div>
      )}

      <div style={{ marginTop: 20 }}>
        <strong>Status atual: {status}</strong>
        <ul style={{ fontSize: 13, background: '#111', color: '#0f0', padding: 12, marginTop: 8, listStyle: 'none' }}>
          {log.map((entry, i) => (
            <li key={i}>{entry}</li>
          ))}
        </ul>
      </div>
    </div>
  )
}

export default App