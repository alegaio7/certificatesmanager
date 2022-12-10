using CertificatesManager.Controllers;
using CertificatesManager.Model;
using Microsoft.AspNetCore.Mvc;

namespace CertificateManager.Tests
{
    public class CertificatesControllerTest : IClassFixture<FixtureCommon<CertificatesController>>
    {
        private FixtureCommon<CertificatesController> _fixture;

        public CertificatesControllerTest(FixtureCommon<CertificatesController> fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_NameCannotBeNull()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.Name = null;

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(CertificatesManager.Api.Strings.REQUEST_NAME_CANNOT_BE_NULL, ((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_NameSizeCannotExceedLimit()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.Name = new string('A', 100);

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(string.Format(CertificatesManager.Api.Strings.REQUEST_NAME_SIZE_CANNOT_EXCEED, Globals.CERT_REQUEST_NAME_SIZE_MAX),
                ((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_NameCannotHaveInvalidCharacters()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.Name = "Test name #";

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(CertificatesManager.Api.Strings.REQUEST_NAME_HAS_INVALID_CHARS, ((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_EmailCannotBeNull()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.Email = null;

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(CertificatesManager.Api.Strings.REQUEST_EMAIL_CANNOT_BE_NULL, ((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_EmailSizeCannotExceedLimit()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.Email = new string('A', 100);

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(string.Format(CertificatesManager.Api.Strings.REQUEST_EMAIL_SIZE_CANNOT_EXCEED, Globals.CERT_REQUEST_EMAIL_SIZE_MAX),
                ((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_EmailShouldBeValid()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.Email = "dummy@domain";

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(CertificatesManager.Api.Strings.REQUEST_EMAIL_IS_INVALID, ((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_OrganizationCannotBeNull()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.Organization = null;

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(CertificatesManager.Api.Strings.REQUEST_ORG_CANNOT_BE_NULL, ((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_OrganizationSizeCannotExceedLimit()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.Organization = new string('A', 100);

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(string.Format(CertificatesManager.Api.Strings.REQUEST_ORG_SIZE_CANNOT_EXCEED, Globals.CERT_REQUEST_ORG_SIZE_MAX),
                ((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_OrganizationCannotHaveInvalidCharacters()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.Organization = "Test name #";

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(CertificatesManager.Api.Strings.REQUEST_ORG_HAS_INVALID_CHARS, ((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_SignerCNCannotBeNull()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.SignerCN = null;

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(CertificatesManager.Api.Strings.REQUEST_SIGNERCN_CANNOT_BE_NULL, ((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_SignerCNSizeCannotExceedLimit()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.SignerCN = new string('A', 100);

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(string.Format(CertificatesManager.Api.Strings.REQUEST_SIGNERCN_SIZE_CANNOT_EXCEED, Globals.CERT_REQUEST_SIGNERCN_SIZE_MAX),
                ((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_SignerCNCannotHaveInvalidCharacters()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.SignerCN = "Test name #";

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(CertificatesManager.Api.Strings.REQUEST_SIGNERCN_HAS_INVALID_CHARS, ((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_SignerEmailCannotBeNull()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.SignerEmail = null;

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(CertificatesManager.Api.Strings.REQUEST_SIGNERMAIL_CANNOT_BE_NULL, ((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_SignerEmailSizeCannotExceedLimit()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.SignerEmail = new string('A', 100);

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(string.Format(CertificatesManager.Api.Strings.REQUEST_SIGNERMAIL_SIZE_CANNOT_EXCEED, Globals.CERT_REQUEST_SIGNEREMAIL_SIZE_MAX),
                ((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_SignerEmailShouldBeValid()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.SignerEmail = "dummy@domain";

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(CertificatesManager.Api.Strings.REQUEST_SIGNERMAIL_IS_INVALID, ((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_SignerOrganizationCannotBeNull()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.SignerOrganization = null;

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(CertificatesManager.Api.Strings.REQUEST_SIGNERORG_CANNOT_BE_NULL, ((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_SignerOrganizationSizeCannotExceedLimit()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.SignerOrganization = new string('A', 100);

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(string.Format(CertificatesManager.Api.Strings.REQUEST_SIGNERORG_SIZE_CANNOT_EXCEED, Globals.CERT_REQUEST_SIGNERORG_SIZE_MAX),
                ((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_SignerOrganizationCannotHaveInvalidCharacters()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.SignerOrganization = "Test name #";

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(CertificatesManager.Api.Strings.REQUEST_SIGNERORG_HAS_INVALID_CHARS, ((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_LocationSizeCannotExceedLimit()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.Location = new string('A', 100);

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(string.Format(CertificatesManager.Api.Strings.REQUEST_LOCATION_SIZE_EXCEEDED, Globals.CERT_REQUEST_LOCATION_SIZE_MAX),
                ((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_LocationCannotHaveInvalidCharacters()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.Location = "Test name #";

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(CertificatesManager.Api.Strings.REQUEST_LOCATION_HAS_INVALID_CHARS, ((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_CountryMustBeValid()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.Country = "XX";

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(CertificatesManager.Api.Strings.REQUEST_COUNTRY_CODE_INVALID, ((BadRequestObjectResult)response).Value);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_ReturnRawOK()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.OutputFormat = Enums.Format.PFXRaw;

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<FileContentResult>(response);
        }

        [Fact]
        public void CreateSelfSignedCertificateForPerson_ReturnBase64EncodedOK()
        {
            // arrange
            var controller = GetController();
            var r = GetPersonCertificateValidRequest();

            // act
            r.OutputFormat = Enums.Format.PFXBase64Encoded;

            // asset
            var response = controller.CreateSelfSignedCertificateForPerson(r);
            Assert.IsType<OkObjectResult>(response);
        }

        #region Private methods
        private CertificatesController GetController()
        {
            var c = new CertificatesController(_fixture.Logger);
            return c;
        }

        private PersonCertificateRequestModel GetPersonCertificateValidRequest()
        {
            var r = new PersonCertificateRequestModel()
            {
                Country = "AR", 
                Email = "johndoe@company.com", 
                Location = "Buenos Aires",
                Name = "John Doe",
                Organization = "My Company", 
                OutputFormat = Enums.Format.PFXBase64Encoded, 
                Password = "12345678", 
                SignatureAlgorithm = Enums.SignatureAlgorithm.ECDSA,
                SignerCN = "mycompany.com - Root Certificate", 
                SignerEmail = "info@mycompany.com",
                SignerOrganization = "mycompany.com"
            };

            return r;
        }
        #endregion
    }
}