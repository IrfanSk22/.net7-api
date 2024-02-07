using System.Net;
using AutoMapper;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_WebApi.Controllers;

[Route("api/VillaNumberAPI")]
[ApiController]
public class VillaNumberAPIController : ControllerBase
{
    private readonly ILogger<VillaNumberAPIController> _logger;
    private readonly IMapper _mapper;
    private readonly IVillaNumberRepository _villaNumRepo;
    private readonly IVillaRepository _villaRepo;
    private APIResponse _response;
    
    public VillaNumberAPIController(ILogger<VillaNumberAPIController> logger, IMapper mapper, 
        IVillaNumberRepository villaNumRepo, IVillaRepository villaRepo)
    {
        _logger = logger;
        _mapper = mapper;
        _villaNumRepo = villaNumRepo;
        _villaRepo = villaRepo;
        _response = new APIResponse();
    }
    
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<APIResponse>> GetAllVillaNumbers()
    {
        try
        {
            IEnumerable<VillaNumber> villaNumberList = await _villaNumRepo.GetAllAsync();
            _response.Result = _mapper.Map<List<VillaNumberDTO>>(villaNumberList);
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _logger.LogError("ERROR - ControllerName: VillaNumberAPI, ActionName: GetAllVillaNumbers");
            _response.IsSuccess = false;
            _response.ErrorMessages
                = new List<string>() { ex.ToString() };
        }
        return _response;
    }
    
    [HttpGet("{id:int}", Name = "GetVillaNumber")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<APIResponse>> GetVillaNumber(int id)
    {
        try
        {
            if (id == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            var villaNumber = await _villaNumRepo.GetAsync(u => u.VillaNo == id);
            if (villaNumber == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }
            _response.Result = _mapper.Map<VillaNumberDTO>(villaNumber);
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _logger.LogError("ERROR - ControllerName: VillaNumberAPI, ActionName: GetVillaNumber");
            _response.IsSuccess = false;
            _response.ErrorMessages
                = new List<string>() { ex.ToString() };
        }
        return _response;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> CreateVillaNumber([FromBody] VillaNumberCreateDTO createDTO)
    {
        try
        {
            if (createDTO == null)
            {
                return BadRequest(createDTO);
            }
            
            if (await _villaNumRepo.GetAsync(u => u.VillaNo == createDTO.VillaNo) != null)
            {
                ModelState.AddModelError("CustomError", "Villa Number already Exists!");
                return BadRequest(ModelState);
            }

            if (await _villaRepo.GetAsync(u => u.Id == createDTO.VillaID) == null)
            {
                ModelState.AddModelError("CustomError", "Villa ID is invalid");
                return BadRequest(ModelState);
            }
            
            VillaNumber villaNumber = _mapper.Map<VillaNumber>(createDTO);
            await _villaNumRepo.CreateAsync(villaNumber);
            
            _response.Result = _mapper.Map<VillaNumberDTO>(villaNumber);
            _response.StatusCode = HttpStatusCode.Created;

            return CreatedAtRoute("GetVillaNumber", new { id = villaNumber.VillaNo }, _response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages = new List<string> { ex.ToString() };

            return _response;
        }
    }
    
    [HttpPut("{id:int}", Name = "UpdateVillaNumber")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<APIResponse>> UpdateVillaNumber(int id, [FromBody] VillaNumberUpdateDTO updateDTO)
    {
        try
        {
            if (updateDTO == null || id != updateDTO.VillaNo)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            
            if (await _villaRepo.GetAsync(u => u.Id == updateDTO.VillaID) == null)
            {
                ModelState.AddModelError("CustomError", "Villa ID is invalid");
                return BadRequest(ModelState);
            }

            VillaNumber villaNumber = _mapper.Map<VillaNumber>(updateDTO);

            await _villaNumRepo.UpdateAsync(villaNumber);
                
            _response.StatusCode = HttpStatusCode.NoContent;
            _response.IsSuccess = true;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages
                = new List<string>() { ex.ToString() };
        }
        return _response;
    }
    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpDelete("{id:int}", Name = "DeleteVillaNumber")]
    public async Task<ActionResult<APIResponse>> DeleteVillaNumber(int id)
    {
        try
        {
            if (id == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
                
            var villaNumber = await _villaNumRepo.GetAsync(u => u.VillaNo == id);
                
            if (villaNumber == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }
                
            await _villaNumRepo.RemoveAsync(villaNumber);
                
            _response.StatusCode = HttpStatusCode.NoContent;
            _response.IsSuccess = true;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages
                = new List<string>() { ex.ToString() };
        }
        return _response;
    }
}
