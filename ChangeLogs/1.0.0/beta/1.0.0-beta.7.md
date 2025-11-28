# 1.0.0-beta.7

## 更新

* 为程序全面添加日志保护
* 现在的日志不会写日期以及调用位置信息，简化日志
* 检查更新时的更新日志过长会显示垂直滚动条
* 将三种卡片合并为一种卡片
  

```mermaid
  flowchart LR

  A[UserDownloadCard] --> D
  B[UserGameCard] --> D
  C[UserTrainerCard] --> D

  D[UserCard]
  ```

* 重画原版游戏图标
* 将所有卡片更新为统一的卡片

## 修复

* 修复下载完修改器后不自动选择下载的修改器

## 移除

* 移除了管理页的进入动画
