using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlakyTest
{
    [Parallelizable(ParallelScope.Self)]                 // roda os testes desta classe em paralelo
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]   // cada teste ganha seu próprio navegador
    public class FlakyLoginTests : PageTest
    {
        private const string BaseUrl = "http://localhost:5173";

        [Test]
        public async Task Login_ShowsGreeting_Flaky([Range(1, 10)] int run)  // vira 10 testes
        {
            // Latência aleatória entre 200 e 1500 ms, como uma API real
            var delay = Random.Shared.Next(200, 1500);
            await Page.GotoAsync($"{BaseUrl}/login?reset=1&delay={delay}");

            await Page.GetByTestId("login-email").FillAsync("cliente@teste.com");
            await Page.GetByTestId("login-senha").FillAsync("Senha@123");
            await Page.GetByTestId("login-submit").ClickAsync();

            // ANTES (flaky):
            // await Page.WaitForTimeoutAsync(800);
            // var visible = await Page.GetByTestId("header-user-name").IsVisibleAsync();
            // Assert.That(visible, Is.True, $"Falhou com delay de {delay} ms");

            // DEPOIS (estável): espera até o elemento aparecer, por até 5 s
            await Expect(Page.GetByTestId("header-user-name")).ToBeVisibleAsync();
        }
    }
}
