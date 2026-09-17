# MesLite 前端 (Vue3 + Vite)

## 1. 安裝 Node.js
確認 `node -v`（建議 18 以上）。沒裝的話去 https://nodejs.org 下載 LTS 版本。

## 2. 安裝套件
在 `mes-frontend` 資料夾下：
```bash
npm install
```

## 3. 重要：改 API 網址
打開 `src/api/http.js`，把 `baseURL` 改成你後端**實際跑起來的網址**（跟 Swagger 網址同一個 host，去掉 `/swagger`）：

```js
const api = axios.create({
  baseURL: 'https://localhost:7000/api'   // <-- 改成你自己的 port
})
```

後端網址在哪裡看：VS 執行後，終端機或瀏覽器網址列會顯示，例如 `https://localhost:7264`。

## 4. 後端 CORS 確認
後端 `Program.cs` 已經設定允許 `http://localhost:5173`（Vite 預設 port），如果 npm run dev 跳出的 port 不是 5173，要回頭改 `Program.cs` 的 CORS 設定加上新的 port。

## 5. 啟動前端
```bash
npm run dev
```
瀏覽器開 `http://localhost:5173`

## 頁面說明
- `/materials` — 物料 CRUD
- `/bom` — BOM 維護（下拉選單自動抓成品/原物料）
- `/workorders` — 工單列表 + 建立工單
- `/workorders/:id` — 工單詳情：BOM 展開需求、自動領料按鈕、報工表單、領料明細、報工紀錄

## 測試流程（跟後端 Swagger 測試對應）
1. `/materials` 先確認種子資料 4 筆物料都在
2. `/bom` 確認腳踏車的 BOM 組成
3. `/workorders` 建一張新工單（或用種子資料那張）
4. 點進工單詳情，按「自動領料」— 狀態會從 DRAFT 變 ISSUED
5. 輸入完工數量，按「送出報工」— 累計滿了狀態自動變 COMPLETED

## 已知限制（可以之後補）
- 沒有登入/權限機制
- 表單沒有防呆驗證（例如工單單號重複）
- 樣式是最簡陽春版，之後可以換成 Element Plus 或 Naive UI 加快 UI 美化
