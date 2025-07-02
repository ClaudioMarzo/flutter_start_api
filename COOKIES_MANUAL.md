# 🍪 INSTRUÇÕES PASSO A PASSO - COOKIES DO YOUTUBE

## 1️⃣ **INSTALAR EXTENSÃO DO NAVEGADOR**

### **Para Chrome:**
1. Acesse: https://chrome.google.com/webstore/detail/get-cookiestxt-locally/cclelndahbckbenkjhflpdbgdldlbecc
2. Clique em "Adicionar ao Chrome"
3. Confirme a instalação

### **Para Firefox:**
1. Acesse: https://addons.mozilla.org/pt-BR/firefox/addon/cookies-txt/
2. Clique em "Adicionar ao Firefox"
3. Confirme a instalação

---

## 2️⃣ **OBTER OS COOKIES**

1. **Acesse** o YouTube: https://youtube.com
2. **Faça login** na sua conta Google
3. **Clique no ícone da extensão** (próximo à barra de endereços)
4. **Clique em "youtube.com"** na lista
5. **Salve o arquivo** como `cookies.txt`

---

## 3️⃣ **CONFIGURAR NO PROJETO**

1. **Mova o arquivo** `cookies.txt` para a raiz do projeto (onde está o arquivo `FlutterStartAPI.sln`)
2. **Verifique** se o arquivo não está vazio (deve ter várias linhas com dados)

---

## 4️⃣ **TESTAR LOCALMENTE**

```bash
# Navegue até o diretório do projeto
cd /home/claudio/projetos/flutter_start_api

# Teste os cookies
yt-dlp --cookies cookies.txt --simulate "https://www.youtube.com/watch?v=dQw4w9WgXcQ"

# Se funcionar, compile e teste a aplicação
dotnet run --project FlutterStart.Apresentation
```

---

## 5️⃣ **CONFIGURAR NO RENDER (PRODUÇÃO)**

### **Gerar Base64:**
```bash
cd /home/claudio/projetos/flutter_start_api
base64 -w 0 cookies.txt
```

### **No Dashboard do Render:**
1. Vá em **Environment Variables**
2. Adicione: 
   - **Nome**: `YOUTUBE_COOKIES_BASE64`
   - **Valor**: [cole o resultado do comando base64]

---

## ✅ **VERIFICAR SE ESTÁ FUNCIONANDO**

### **Sinais de sucesso:**
- Arquivo `cookies.txt` existe e não está vazio
- Comando de teste do yt-dlp funciona
- Aplicação não retorna erro 429

### **Se ainda der erro 429:**
- Aguarde 10-15 minutos
- Renove os cookies (repita o processo)
- Teste com URL diferente

---

## 🔄 **MANUTENÇÃO**

- **Renove os cookies** a cada 1-2 semanas
- **Mantenha-se logado** no YouTube
- **Não commite** o arquivo cookies.txt no git

---

## 🆘 **PROBLEMAS COMUNS**

**"Cookies not found"** → Verifique se o arquivo está na raiz do projeto
**"Still getting 429"** → Aguarde mais tempo, renove cookies
**"Empty cookies file"** → Refaça o processo de extração
