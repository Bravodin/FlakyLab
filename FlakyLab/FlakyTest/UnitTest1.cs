using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace FlakyTest
{

    public class LoginTests : PageTest
    {
        private  const string BaseUrl = "http://localhost:5173/login";
       
        [Test]
        public async Task Login_WithValidUser_ShowsGreeting()
        {

            // Arrange: abre a tela de login com os dados zerados
            await Page.GotoAsync($"{BaseUrl}");


            // Act: preenche e envia
            await Page.GetByTestId("login-email").FillAsync("cliente@teste.com");
            await Page.GetByTestId("login-senha").FillAsync("Senha@123");
            await Page.GetByTestId("login-submit").ClickAsync();

            // Assert: espera a saudação aparecer (até 5 s, sem sleep)
            await Expect(Page.GetByTestId("header-user-name")).ToContainTextAsync("Olá");

            // Mantém o browser aberto até clicar em "Resume" no Playwright Inspector
            await Page.PauseAsync();
        }
    }
}
