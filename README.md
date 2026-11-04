### 1. Pre Requisitos

- .NET 8 SDK
- The app under test running on `http://localhost:5173`:
  [mock-marketplace-iafirst](https://github.com/Bravodin/mock-marketplace-iafirst),
  um e-commerce criado para laboratório de teste.

```bash
git clone https://github.com/Bravodin/mock-marketplace-iafirst.git
cd mock-marketplace-iafirst
npm install
npm run dev
```

# 🧪 FlakyLab

Um laboratório para entender, reproduzir e **corrigir testes flaky** (instáveis) em testes end-to-end com **Playwright + NUnit + .NET 8**.

> Teste flaky é aquele que às vezes passa e às vezes falha, sem nenhuma mudança no código. Quase sempre o culpado é **timing**.

---

## 🎯 O que este projeto demonstra

O teste `FlakyLoginTests` simula uma API com **latência aleatória (200–1500 ms)** e roda o mesmo cenário de login **10 vezes em paralelo**. Assim dá para ver, na prática, a diferença entre as duas abordagens:

### ❌ Antes: espera fixa (flaky)

```csharp
await Page.WaitForTimeoutAsync(800);
var visible = await Page.GetByTestId("header-user-name").IsVisibleAsync();
Assert.That(visible, Is.True);
```

Se a resposta demora mais que 800 ms, o teste falha. Se demora menos, o tempo que sobra é desperdiçado.

### ✅ Depois: espera baseada em condição (estável)

```csharp
await Expect(Page.GetByTestId("header-user-name")).ToBeVisibleAsync();
```

O Playwright **tenta de novo automaticamente** até o elemento aparecer (por até 5 s, o padrão). Não precisa de `sleep`, e o teste fica rápido e confiável.

---

## 🗂️ Estrutura

```
FlakyLab/
├── FlakyLab.slnx               # Solução
└── FlakyTest/
    ├── FlakyTest.csproj        # Projeto de testes (.NET 8)
    ├── test.runsettings        # Configuração do Playwright (Chromium, com janela)
    ├── UnitTest1.cs            # LoginTests: login simples, com Playwright Inspector
    └── FlakyLoginTests.cs      # 10 execuções paralelas com latência aleatória
```

| Teste | O que faz |
|---|---|
| `LoginTests.Login_WithValidUser_ShowsGreeting` | Faz login e confere a saudação "Olá". Chama `Page.PauseAsync()` no final para você inspecionar a página. |
| `FlakyLoginTests.Login_ShowsGreeting_Flaky(1..10)` | Gera 10 casos com `[Range(1, 10)]`, cada um com seu próprio navegador e um `delay` aleatório. |

---

## 🧰 Stack

- [.NET 8](https://dotnet.microsoft.com/)
- [NUnit 4](https://nunit.org/)
- [Microsoft.Playwright.NUnit](https://playwright.dev/dotnet/)
- [Coverlet](https://github.com/coverlet-coverage/coverlet) (cobertura)

---

## 🚀 Como rodar

### 1. Pré-requisitos

- .NET 8 SDK
- **A aplicação de login rodando em `http://localhost:5173`** (ela não faz parte deste repositório). A página precisa ter:
  - os `data-testid`: `login-email`, `login-senha`, `login-submit` e `header-user-name`
  - suporte aos parâmetros de query `?reset=1` (reseta o estado) e `?delay=<ms>` (simula latência)

### 2. Compilar e instalar os navegadores do Playwright

```powershell
cd FlakyLab
dotnet build
pwsh FlakyTest/bin/Debug/net8.0/playwright.ps1 install
```

### 3. Executar os testes

```powershell
# Todos os testes
dotnet test

# Só a demonstração de flakiness
dotnet test --filter "FullyQualifiedName~FlakyLoginTests"
```

> ⚠️ `LoginTests` chama `Page.PauseAsync()` e fica parado até você clicar em **Resume** no Playwright Inspector. Para rodar sem interação, filtre esse teste ou remova a pausa.

---

## ⚙️ Configuração

O arquivo `test.runsettings` roda o **Chromium com janela visível** (`Headless=false`). Para rodar em CI ou sem abrir o navegador:

```powershell
dotnet test -- Playwright.LaunchOptions.Headless=true
```

Outras opções úteis:

```powershell
# Rodar em outro navegador
dotnet test -- Playwright.BrowserName=firefox

# Deixar as ações em câmera lenta (ms) para acompanhar
dotnet test -- Playwright.LaunchOptions.SlowMo=500
```

---

## 🧪 Experimente

1. Em `FlakyLoginTests.cs`, descomente o bloco **ANTES** e comente a linha do `Expect`.
2. Rode `dotnet test` algumas vezes e veja alguns dos 10 casos falharem, sempre os que têm `delay > 800 ms`.
3. Volte para a versão com `Expect(...)` e veja todos passarem. ✅

---

## 📚 Lições

- **Nunca use `sleep`/`WaitForTimeout` para esperar a UI.** Espere uma *condição*.
- Prefira **web-first assertions** (`Expect(...).ToBeVisibleAsync()`, `ToContainTextAsync()`), que já fazem retry.
- Use **`data-testid`** como seletor estável, independente de CSS e texto.
- **Isole cada teste** (`InstancePerTestCase` + `?reset=1`) para que um não interfira no outro quando rodam em paralelo.
