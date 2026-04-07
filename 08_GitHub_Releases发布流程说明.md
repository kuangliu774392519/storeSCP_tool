# GitHub Releases 发布流程说明

最后更新时间：2026-04-07  
适用仓库：`git@github.com:kuangliu774392519/storeSCP_tool.git`  
文档类型：活文档

## 1. 目的

本文档用于明确：

- 现场同事应该从哪里下载“可直接运行版本”
- 维护者如何从源码仓库生成并发布绿色版压缩包

## 2. 给现场同事的结论

如果目标是“下载后直接运行”，正确方式是：

- 去 GitHub 仓库的 `Releases` 页面下载绿色版 zip

不建议：

- `git clone` 源码后自己编译
- 在仓库主页点击 `Code -> Download ZIP` 后直接尝试运行

原因：

- 源码仓库默认不包含 `dist\win-x64` 发布目录
- 同事直接需要的是发布包，而不是源码

## 3. 推荐下载入口

推荐入口：

- GitHub Releases 页面

仓库地址：

- [storeSCP_tool](https://github.com/kuangliu774392519/storeSCP_tool)

Releases 页面：

- [Releases](https://github.com/kuangliu774392519/storeSCP_tool/releases)

## 4. 当前发布物形式

发布包命名规则：

```text
StorescpTool_win-x64_portable_<version>.zip
```

同时会附带一个哈希文件：

```text
StorescpTool_win-x64_portable_<version>.sha256.txt
```

## 5. 本地手工打包方式

本地可使用以下脚本：

- [Publish-StorescpTool.ps1](C:\Users\wik\Desktop\storescp工具\scripts\Publish-StorescpTool.ps1)

示例：

```powershell
.\scripts\Publish-StorescpTool.ps1 -Configuration Release -Runtime win-x64 -CreatePortableZip -PackageVersion 0.1.2
```

默认行为：

- 发布目录输出到 `dist\win-x64`
- 绿色版 zip 输出到 `.artifacts\portable`

## 6. GitHub 自动发布方式

仓库中已新增工作流：

- [.github/workflows/portable-release.yml](C:\Users\wik\Desktop\storescp工具\.github\workflows\portable-release.yml)

工作流行为：

- 当推送 tag `v*` 时：
  - 自动 restore
  - 自动 test
  - 自动 publish
  - 自动打绿色版 zip
  - 自动上传 GitHub Release 附件
- 当手动触发 `workflow_dispatch` 时：
  - 自动构建并生成 workflow artifact

## 7. 推荐正式发版流程

建议维护者按以下顺序操作：

1. 本地确认工作区干净
2. 运行测试
3. 如有需要，本地先打一个绿色包验证
4. 提交代码并推送 `main`
5. 创建版本 tag，例如：

```powershell
git tag v0.1.2
git push origin v0.1.2
```

6. 等待 GitHub Actions 完成
7. 到 Releases 页面检查 zip 和 sha256 文件是否都已生成

## 8. 第一次启用后的检查点

第一次使用 GitHub 自动发版时，建议重点检查：

1. Actions 是否成功运行
2. Release 是否成功创建
3. zip 是否能正常下载
4. 解压后 exe 是否能正常启动
5. 默认参数是否正确

## 9. 常见误区

### 9.1 从源码 ZIP 里找 exe

不推荐。

源码包不是正式交付包，默认不包含发布产物。

### 9.2 把 `dist` 提交到 Git

不推荐。

原因：

- 二进制文件会让仓库迅速膨胀
- Git 历史会变差
- 每次发版 diff 噪声很大

当前仓库已经通过 `.gitignore` 排除了 `dist/`。

## 10. 当前结论

以后对现场同事应该统一这样描述：

- 源码在 Git 仓库
- 可直接运行版本在 GitHub Releases

不要再用“克隆源码仓库后直接运行”的方式做交付。
