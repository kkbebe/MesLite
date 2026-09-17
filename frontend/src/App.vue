<script setup>
import { ref } from 'vue'

const openGroups = ref({ data: true, prod: true })
function toggle(key) {
  openGroups.value[key] = !openGroups.value[key]
}
</script>

<template>
  <div class="layout">
    <aside class="sidebar">
      <div class="logo">MesLite</div>

      <div class="menu-group">
        <div class="menu-title" @click="toggle('data')">
          <span>📋 數據管理</span>
          <span>{{ openGroups.data ? '▾' : '▸' }}</span>
        </div>
        <div v-show="openGroups.data" class="menu-items">
          <router-link to="/materials" class="menu-item">物料</router-link>
          <router-link to="/bom" class="menu-item">BOM 管理</router-link>
        </div>
      </div>

      <div class="menu-group">
        <div class="menu-title" @click="toggle('prod')">
          <span>🏭 生產作業</span>
          <span>{{ openGroups.prod ? '▾' : '▸' }}</span>
        </div>
        <div v-show="openGroups.prod" class="menu-items">
          <router-link to="/workorders" class="menu-item">工單管理</router-link>
          <router-link to="/issue" class="menu-item">領料</router-link>
          <router-link to="/report" class="menu-item">報工</router-link>
        </div>
      </div>
    </aside>

    <div class="content">
      <header class="topbar">MesLite 生產管理系統</header>
      <main class="page">
        <router-view />
      </main>
    </div>
  </div>
</template>

<style>
.layout { display: flex; min-height: 100vh; }

.sidebar {
  width: 220px;
  background: #1f2937;
  color: #d1d5db;
  flex-shrink: 0;
}
.logo {
  padding: 18px 20px;
  font-size: 20px;
  font-weight: bold;
  color: #fff;
  border-bottom: 1px solid #374151;
}
.menu-group { border-bottom: 1px solid #374151; }
.menu-title {
  padding: 12px 20px;
  cursor: pointer;
  font-weight: bold;
  display: flex;
  justify-content: space-between;
  color: #fff;
}
.menu-title:hover { background: #374151; }
.menu-items { display: flex; flex-direction: column; }
.menu-item {
  padding: 10px 20px 10px 36px;
  color: #d1d5db;
  font-size: 14px;
}
.menu-item:hover { background: #374151; color: #fff; }
.menu-item.router-link-active {
  background: #2563eb;
  color: #fff;
}

.content { flex: 1; display: flex; flex-direction: column; }
.topbar {
  background: #fff;
  padding: 14px 24px;
  font-size: 16px;
  font-weight: bold;
  border-bottom: 1px solid #e5e7eb;
}
.page { max-width: 1200px; width: 100%; margin: 0 auto; padding: 20px; }
</style>
