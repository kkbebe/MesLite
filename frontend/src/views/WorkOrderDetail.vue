<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import api from '../api/http'

const route = useRoute()
const woId = Number(route.params.id)

const detail = ref(null)
const errorMsg = ref('')

async function loadDetail() {
  errorMsg.value = ''
  try {
    const res = await api.get(`/workorders/${woId}/detail`)
    detail.value = res.data
  } catch (e) {
    errorMsg.value = '讀取失敗：' + (e.response?.data ?? e.message)
  }
}

onMounted(loadDetail)
</script>

<template>
  <div v-if="detail">
    <h2>
      工單 {{ detail.workOrder.woNumber }}
      <span class="status-tag" :class="'status-' + detail.workOrder.status">{{ detail.workOrder.status }}</span>
    </h2>
    <p>成品：{{ detail.workOrder.productName }}　計畫數量：{{ detail.workOrder.targetQty }}</p>
    <p>已報工累計：{{ detail.totalReported }} / {{ detail.workOrder.targetQty }}　剩餘：{{ detail.remainingQty }}</p>

    <div class="card">
      <h3>BOM 展開需求</h3>
      <table>
        <thead>
          <tr><th>物料</th><th>單位用量</th><th>需求總量</th><th>目前庫存</th><th>是否足夠</th></tr>
        </thead>
        <tbody>
          <tr v-for="b in detail.bomRequirement" :key="b.materialId">
            <td>{{ b.name }} ({{ b.itemCode }})</td>
            <td>{{ b.requiredPerUnit }}</td>
            <td>{{ b.requiredTotal }}</td>
            <td>{{ b.currentStock }}</td>
            <td>{{ b.currentStock >= b.requiredTotal ? '✅ 足夠' : '❌ 不足' }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="card">
      <h3>已領料明細</h3>
      <table>
        <thead><tr><th>物料</th><th>數量</th><th>領料人</th><th>時間</th></tr></thead>
        <tbody>
          <tr v-for="t in detail.issuedTransactions" :key="t.id">
            <td>{{ t.materialName }}</td>
            <td>{{ t.qty }}</td>
            <td>{{ t.operatorName }}</td>
            <td>{{ new Date(t.createdAt).toLocaleString() }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="card">
      <h3>報工紀錄</h3>
      <table>
        <thead><tr><th>作業員</th><th>完工數量</th><th>報工時間</th></tr></thead>
        <tbody>
          <tr v-for="r in detail.workReports" :key="r.id">
            <td>{{ r.operatorName }}</td>
            <td>{{ r.completedQty }}</td>
            <td>{{ new Date(r.reportTime).toLocaleString() }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
  <p v-else-if="errorMsg" class="error-msg">{{ errorMsg }}</p>
  <p v-else>載入中...</p>
</template>
