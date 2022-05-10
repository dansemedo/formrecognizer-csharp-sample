using Microsoft.AspNetCore.Mvc;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.Extensions.Configuration.Json;

namespace FormRecognizerAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CnhDocumentController : ControllerBase
    {
        private readonly ILogger<CnhDocumentController> _logger;

        public CnhDocumentController(ILogger<CnhDocumentController> logger)
        {
            _logger = logger;
        }

        [HttpPost("GetDocumentData")]
        public async Task<IActionResult> PostDocumentDataAsync(string docUrl)
        {
            var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json").Build();

            string endpoint = config.GetValue<string>("Endpoint");
            string key = config.GetValue<string>("Key");
            AzureKeyCredential credential = new AzureKeyCredential(key);
            DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(endpoint), credential);
            string firstName = "";
            string lastName = "";
            string region = "";
            string sex = "";

            //FOTO PARA USAR DE EXEMPLO "https://raw.githubusercontent.com/Azure-Samples/cognitive-services-REST-api-samples/master/curl/form-recognizer/DriverLicense.png"


            Uri fileUri = new Uri(docUrl);

            AnalyzeDocumentOperation operation = await client.StartAnalyzeDocumentFromUriAsync("prebuilt-idDocument", fileUri);

            await operation.WaitForCompletionAsync();

            AnalyzeResult result = operation.Value;

            _logger.LogInformation("Form Recognizer got the OCR from the document...");

            for (int i = 0; i < result.Documents.Count; i++)
            {
                //  Console.WriteLine($"Document {i}:");

                AnalyzedDocument document = result.Documents[i];

                if (document.Fields.TryGetValue("FirstName", out DocumentField? firstNameField))
                {
                    if (firstNameField.ValueType == DocumentFieldType.String)
                    {
                        firstName = firstNameField.AsString();
                        // Console.WriteLine($"First Name: '{firstName}', with confidence {firstNameField.Confidence}");
                        _logger.LogInformation($"First Name '{firstName}' with confidence   {firstNameField.Confidence}...");
                    }
                }

                if (document.Fields.TryGetValue("LastName", out DocumentField? lastNameField))
                {
                    if (lastNameField.ValueType == DocumentFieldType.String)
                    {
                        lastName = lastNameField.AsString();
                        //Console.WriteLine($"Last Name: '{lastName}', with confidence {lastNameField.Confidence}");
                        _logger.LogInformation($"Last Name '{lastName}' with confidence   {lastNameField.Confidence}...");
                    }
                }
                if (document.Fields.TryGetValue("Region", out DocumentField? RegionField))
                {
                    if (RegionField.ValueType == DocumentFieldType.String)
                    {
                        region = RegionField.AsString();
                        _logger.LogInformation($"Region: '{region}', with confidence {RegionField.Confidence}");
                    }
                }
                if (document.Fields.TryGetValue("Sex", out DocumentField? SexField))
                {
                    if (SexField.ValueType == DocumentFieldType.String)
                    {
                        sex = SexField.AsString();
                        _logger.LogInformation($"Sex: '{sex}', with confidence {SexField.Confidence}");
                    }
                }

            }

            return Ok("MOTORISTA  -   NOME: " + firstName + "  | SOBRENOME: " + lastName + "  | SEXO: " + sex + "  | REGIAO:  " + region);
        }

    }
}