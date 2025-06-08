# Sistema de Limpeza Automática - WebP Converter

## 🎯 Objetivo

Implementar limpeza automática de arquivos para manter o sistema gratuito funcionando de forma eficiente, removendo arquivos após **5 minutos** para evitar acúmulo de armazenamento.

## ✅ Implementação Completa

### **1. Serviço BackgroundService Aprimorado**

**Arquivo:** `src/conversor/Worker/LimparArquivos.cs`

#### **Configurações Principais:**
```csharp
private const int MinutosParaLimpeza = 5; // 5 minutos para sistema gratuito
private const string PastaUploads = "uploads";
private const string PastaConvertidos = "convertidos";
```

#### **Funcionalidades:**
- ✅ **Limpeza a cada 2 minutos** (execução do serviço)
- ✅ **Remove arquivos > 5 minutos** (critério de limpeza)
- ✅ **Logs detalhados** com informações de MB liberados
- ✅ **Tratamento de erros** robusto
- ✅ **Cálculo de espaço liberado** em tempo real

### **2. Lógica de Limpeza Avançada**

#### **A. Pastas de Upload (Arquivos Originais):**
```csharp
// Remove pastas de sessão com arquivos originais
await LimparPasta(PastaUploads, timestampLimpeza, ref totalBytesLiberados);
```

#### **B. Pastas de Convertidos (Arquivos Processados):**
```csharp
// Remove pastas de sessão com arquivos convertidos
await LimparPasta(PastaConvertidos, timestampLimpeza, ref totalBytesLiberados);
```

#### **C. Arquivos ZIP (Downloads):**
```csharp
// Remove arquivos ZIP antigos diretamente
await LimparArquivosZip(timestampLimpeza, ref totalBytesLiberados);
```

### **3. Logs e Monitoramento**

#### **Logs de Início:**
```
🧹 Serviço de Limpeza Automática iniciado - Intervalo: 5 minutos
📁 Pasta de uploads criada: /path/to/uploads
📁 Pasta de convertidos criada: /path/to/convertidos
```

#### **Logs de Limpeza:**
```
🧹 Limpeza concluída: 3 pastas removidas, 15.42 MB liberados
🗑️ Pasta removida: /path/to/session123 (5242880 bytes)
🗑️ Arquivo ZIP removido: /path/to/session456.zip (1048576 bytes)
```

#### **Logs de Erro:**
```
❌ Erro durante a limpeza automática de arquivos
⚠️ Erro ao remover pasta: /path/to/session789
```

## 📐 Cronograma de Execução

### **Ciclo de Vida dos Arquivos:**

```
📤 Upload (00:00)
    ↓
🔄 Processamento (00:01-00:03)
    ↓
📦 Download disponível (00:03)
    ↓
⏰ Primeira verificação (00:02)
    ↓
⏰ Segunda verificação (00:04)
    ↓
🗑️ REMOÇÃO AUTOMÁTICA (00:05)
```

### **Frequência de Limpeza:**
- **Execução**: A cada 2 minutos
- **Critério**: Remove arquivos > 5 minutos
- **Retry**: 1 minuto em caso de erro

## 🎨 Interface do Usuário

### **1. Notificação no Header**
```html
<div class="free-service-notice">
    <div class="notice-content">
        <i data-feather="clock"></i>
        <span><strong>Serviço Gratuito:</strong> Seus arquivos são automaticamente removidos após 5 minutos para manter o sistema disponível para todos.</span>
    </div>
</div>
```

**Visual:** Barra amarela/dourada abaixo do header principal.

### **2. Política de Privacidade Atualizada**
- ✅ **Remoção automática:** 5 minutos para todos os arquivos
- ✅ **Limpeza frequente:** A cada 2 minutos
- ✅ **Sistema gratuito:** Justificativa clara
- ✅ **Sem backup:** Garantia de privacidade

## 📊 Benefícios do Sistema

### **Para o Sistema:**
- ✅ **Armazenamento controlado** - Evita acúmulo
- ✅ **Performance mantida** - Sistema sempre ágil
- ✅ **Disponibilidade garantida** - Serviço sempre online
- ✅ **Custos controlados** - Viável para ser gratuito

### **Para o Usuário:**
- ✅ **Privacidade garantida** - Arquivos não ficam armazenados
- ✅ **Transparência total** - Usuário sabe das regras
- ✅ **Serviço gratuito** - Sem custos
- ✅ **Tempo suficiente** - 5 minutos para download

### **Para Monitoramento:**
- ✅ **Logs detalhados** - Acompanhamento em tempo real
- ✅ **Métricas de limpeza** - MB liberados por execução
- ✅ **Detecção de erros** - Problemas identificados rapidamente
- ✅ **Performance tracking** - Tempo de execução monitorado

## 🔧 Configuração Técnica

### **1. Registro do Serviço**
```csharp
// Program.cs
builder.Services.AddHostedService<LimparArquivos>();
```

### **2. Injeção de Dependência**
```csharp
public LimparArquivos(ILogger<LimparArquivos> logger)
{
    _logger = logger;
}
```

### **3. Estrutura de Pastas**
```
wwwroot/
├── uploads/           # Arquivos originais (por sessionId)
│   ├── session-123/
│   ├── session-456/
│   └── ...
├── convertidos/       # Arquivos processados (por sessionId)
│   ├── session-123/
│   ├── session-456/
│   ├── session-123.zip
│   └── ...
```

## ⚡ Performance

### **Execução Eficiente:**
- **Intervalo**: 2 minutos (não sobrecarrega)
- **Critério preciso**: LastWriteTime (mais confiável)
- **Cálculo otimizado**: Tamanho calculado antes da remoção
- **Logs controlados**: Debug apenas quando necessário

### **Tratamento de Erros:**
- **Try-catch individual**: Falha em uma pasta não afeta outras
- **Retry automático**: 1 minuto de delay em caso de erro
- **Logs de warning**: Problemas registrados sem parar o serviço

## 🖥️ **Resultado Final**

**URL:** http://localhost:5050

### **Sistema Funcionando:**
1. ✅ **Upload** → Arquivos salvos em sessões únicas
2. ✅ **Processamento** → Conversão para WebP
3. ✅ **Download** → ZIP disponível para baixar
4. ✅ **Limpeza automática** → Remoção após 5 minutos
5. ✅ **Notificação clara** → Usuário informado das regras

### **Monitoramento:**
- **Logs no console** da aplicação
- **Métricas de espaço** liberado
- **Frequência controlada** de execução
- **Sistema sustentável** para uso gratuito

O sistema agora é **totalmente sustentável** para um serviço gratuito, mantendo a privacidade e performance! 🎯 