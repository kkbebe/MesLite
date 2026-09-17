import axios from 'axios'

// 這裡改成你 .NET API 實際跑起來的網址（看 launchSettings.json 或啟動時終端機顯示的網址）
const api = axios.create({
  baseURL: 'https://localhost:7131/api'
})

export default api
