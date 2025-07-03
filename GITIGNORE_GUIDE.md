# Guia de Arquivos para .gitignore

## ✅ Arquivos ADICIONADOS ao .gitignore

### 🔒 **Arquivos de Segurança (CRÍTICOS - NUNCA commitar)**
```
# YouTube cookies - NEVER commit this file!
cookies.txt
*.cookies
cookies_base64.txt

# Environment files with sensitive data
.env
.env.local
.env.production
.env.staging
*.env
```

### 📁 **Diretórios de Downloads e Temporários**
```
# Downloads folder and test downloads
downloads/
test_downloads/
**/downloads/*
**/test_downloads/*

# yt-dlp temporary files
*.part
*.ytdl
*.temp
```

### 🔧 **Configurações de Desenvolvimento**
```
# IDE and editor files
*.swp
*.swo
*~
.vscode/settings.json
.vscode/tasks.json

# Azure deployment files (if contains sensitive data)
.azure/
azure.yml

# Docker build context sensitive files
Dockerfile.prod
docker-compose.override.yml
```

### 📊 **Arquivos de Log e Dados de Runtime**
```
# Logs
logs/
*.log
npm-debug.log*
yarn-debug.log*
yarn-error.log*
lerna-debug.log*

# Runtime data
pids
*.pid
*.seed
*.pid.lock

# Temporary folders
tmp/
temp/
```

### ⚙️ **Configurações de Aplicação**
```
# Application specific
appsettings.*.json
!appsettings.json
!appsettings.Development.json
```

## ⚠️ **ARQUIVOS JÁ REMOVIDOS do Git**

### `cookies.txt` 
- **Status**: ✅ Removido do controle de versão
- **Ação realizada**: `git rm --cached cookies.txt`
- **Motivo**: Contém dados de autenticação sensíveis

## 🟢 **Arquivos SEGUROS para Commit**

### Configurações de Projeto
- ✅ `appsettings.json` (configurações básicas)
- ✅ `appsettings.Development.json` (configurações de desenvolvimento)
- ✅ `.vscode/launch.json` (configuração de debug - geralmente segura)

### Scripts e Documentação
- ✅ `setup-render-cookies.sh` (script público)
- ✅ `test-downloads.sh` (script de teste)
- ✅ `RENDER_COOKIES_GUIDE.md` (documentação)
- ✅ `CHANGELOG.md` (histórico de mudanças)

### Executáveis
- ✅ `yt-dlp_linux` (executável público)
- ✅ `yt-dlp_windows.exe` (executável público)

## 🔍 **Verificações Importantes**

### Antes de Cada Commit
```bash
# Verificar arquivos que serão commitados
git status

# Verificar se não há dados sensíveis
git diff --cached

# Verificar se cookies não estão sendo adicionados
ls -la | grep -i cookie
```

### Comando para Limpar Cache do Git
```bash
# Se algum arquivo sensível foi commitado acidentalmente
git rm --cached <arquivo-sensivel>
git commit -m "Remove arquivo sensível do controle de versão"
```

## 🚨 **NUNCA Commitar**

### Dados de Autenticação
- ❌ `cookies.txt`
- ❌ `cookies_base64.txt`
- ❌ Qualquer arquivo `.env` com dados sensíveis
- ❌ Chaves de API
- ❌ Tokens de acesso
- ❌ Senhas ou credenciais

### Dados Pessoais
- ❌ Downloads de vídeos/áudio
- ❌ Logs com informações pessoais
- ❌ Configurações locais específicas

### Arquivos Temporários
- ❌ Arquivos `.part` (downloads incompletos)
- ❌ Logs de debug
- ❌ Cache de builds

## 📋 **Checklist para Deploy no Render**

### Antes do Push
- [ ] ✅ Arquivo `cookies.txt` não está no git
- [ ] ✅ Variável `YOUTUBE_COOKIES_BASE64` configurada no Render
- [ ] ✅ Arquivos de download não estão no repositório
- [ ] ✅ Logs sensíveis removidos
- [ ] ✅ `.gitignore` atualizado

### Configurações no Render
- [ ] ✅ Variável `YOUTUBE_COOKIES_BASE64` definida
- [ ] ✅ Outras variáveis de ambiente necessárias
- [ ] ✅ Build e deploy funcionando

---

## 💡 **Dica Extra**

Para verificar se há arquivos sensíveis no seu repositório:

```bash
# Buscar por padrões sensíveis
git log --all --full-history -- "*cookie*"
git log --all --full-history -- "*.env"

# Verificar tamanho dos arquivos commitados
git ls-tree -r -t -l --full-name HEAD | sort -n -k 4
```
