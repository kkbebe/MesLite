import { createRouter, createWebHistory } from 'vue-router'
import Materials from '../views/Materials.vue'
import Bom from '../views/Bom.vue'
import WorkOrders from '../views/WorkOrders.vue'
import WorkOrderDetail from '../views/WorkOrderDetail.vue'
import Report from '../views/Report.vue'
import Issue from '../views/Issue.vue'

const routes = [
  { path: '/', redirect: '/materials' },
  { path: '/materials', component: Materials },
  { path: '/bom', component: Bom },
  { path: '/workorders', component: WorkOrders },
  { path: '/workorders/:id', component: WorkOrderDetail, props: true },
  { path: '/report', component: Report },
  { path: '/issue', component: Issue }
]

export default createRouter({
  history: createWebHistory(),
  routes
})
