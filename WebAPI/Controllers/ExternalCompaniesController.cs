using Microsoft.AspNetCore.Mvc;
using Entities.DbModels;


namespace WebAPI.Services
{
    [Route("api/external-companies")] // endpoint de güncellendi
    [ApiController]
    public class ExternalCompaniesController : ControllerBase
    {
        private readonly CompanyService _companyService;

        public ExternalCompaniesController(CompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companies = await _companyService.GetAllCompaniesAsync();
            return Ok(companies);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var company = await _companyService.GetCompanyByIdAsync(id);
            if (company == null)
                return NotFound();

            return Ok(company);
        }
    }
}
