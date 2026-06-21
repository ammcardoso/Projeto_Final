using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Projeto1_IF.Models;
using System.Data;

namespace Projeto1_IF.Controllers;

[Authorize]
public class TbPacienteController(db_IFContext context, ILogger<HomeController> logger) : Controller
{
    private readonly db_IFContext _context = context;
    private readonly ILogger<HomeController> _logger = logger;

    // GET: TbPaciente
    //Adriana Cardoso 
    public async Task<IActionResult> Index()
    {
        string email = User.Identity!.Name!;
        AspNetUser aspNetUser = await _context.AspNetUsers.AsNoTracking().SingleOrDefaultAsync(x => x.Email == email) ?? throw new Exception($"[ ERROR ] - Usuário {email} não encontrado");
        TbProfissional tbProfissional = await _context.TbProfissionals.AsNoTracking().SingleOrDefaultAsync(x => x.IdUser == aspNetUser.Id) ?? throw new Exception($"[ ERROR ] - Profissional para o usuário {email} não encontrado");

        List<TbPaciente> pacientes = 
            await _context.TbPacientes
                .Include(x => x.IdCidadeNavigation)
                .Include(x => x.TbMedicoPacientes)
                .Where(x => x.TbMedicoPacientes.Any(y => y.IdProfissional == tbProfissional.IdProfissional))
                .ToListAsync();

        return View(pacientes);
    }

    // GET: TbPaciente/Details/5
    //Adriana Cardoso 
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var tbPaciente = await _context.TbPacientes
            .Include(t => t.IdCidadeNavigation)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.IdPaciente == id);
        if (tbPaciente == null)
        {
            return NotFound();
        }
        return View(tbPaciente);
    }

    // GET: TbPaciente/Create
    //Adriana Cardoso 
    public IActionResult Create()
    {
        ViewData["IdCidade"] = new SelectList(_context.TbCidades, "IdCidade", "Nome");
        return View();
    }

    // POST: TbPaciente/Create
    //Adriana Cardoso 
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nome,Rg,Cpf,DataNascimento,NomeResponsavel,Sexo,Etnia,Endereco,Bairro,IdCidade,TelResidencial,TelComercial,TelCelular,Profissao,FlgAtleta,FlgGestante")] TbPaciente tbPaciente)
    {
        try
        {
            if (ModelState.IsValid)
            {
                string email = User.Identity!.Name!;
                AspNetUser aspNetUser = await _context.AspNetUsers.AsNoTracking().SingleOrDefaultAsync(x => x.Email == email) ?? throw new Exception($"[ ERROR ] - Usuário {email} não encontrado");
                TbProfissional tbProfissional = await _context.TbProfissionals.AsNoTracking().SingleOrDefaultAsync(x => x.IdUser == aspNetUser.Id) ?? throw new Exception($"[ ERROR ] - Profissional para o usuário {email} não encontrado");

                await _context.AddAsync(tbPaciente);
                await _context.SaveChangesAsync();

                TbMedicoPaciente medicoPaciente = new()
                {
                    IdPaciente = tbPaciente.IdPaciente,
                    IdProfissional = tbProfissional.IdProfissional
                };

                await _context.AddAsync(medicoPaciente);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError(string.Empty, $"Ocorreu um erro ao criar o paciente: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocorreu um erro inesperado ao criar o paciente.");
            ModelState.AddModelError(string.Empty, $"Ocorreu um erro inesperado: {ex.Message}");
        }

        ViewData["IdCidade"] = new SelectList(_context.TbCidades, "IdCidade", "Nome", tbPaciente.IdCidade);
        return View(tbPaciente);
    }

    // GET: TbPaciente/Edit/5
    //Adriana Cardoso 
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return RedirectToAction("Error", "Home");
        }
        var tbPaciente = await _context.TbPacientes.FindAsync(id);
        if (tbPaciente == null)
        {
            return NotFound();
        }
        ViewData["IdCidade"] = new SelectList(_context.TbCidades, "IdCidade", "Nome", tbPaciente.IdCidade);
        return View(tbPaciente);
    }

    // POST: TbPaciente/Edit/5
    //Adriana Cardoso 
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var tbPaciente = await _context.TbPacientes.FirstOrDefaultAsync(s => s.IdPaciente == id);
        if (tbPaciente == null)
        {
            return NotFound();
        }
        if (await TryUpdateModelAsync<TbPaciente>(
            tbPaciente,
            "",
            s => s.IdPaciente,
            s => s.Nome,
            s => s.Rg,
            s => s.Cpf,
            s => s.DataNascimento,
            s => s.NomeResponsavel,
            s => s.Sexo,
            s => s.Etnia,
            s => s.Endereco,
            s => s.Bairro,
            s => s.IdCidade,
            s => s.TelResidencial,
            s => s.TelComercial,
            s => s.TelCelular,
            s => s.Profissao,
            s => s.FlgAtleta,
            s => s.FlgGestante))
        {
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("",
                    "Não foi possível salvar as alterações. " + ex.ToString());
            }
        }
        ViewData["IdCidade"] = new SelectList(_context.TbCidades, "IdCidade", "Nome", tbPaciente.IdCidade);
        return View(tbPaciente);
    }

    // GET: TbPaciente/Delete/5
    //Adriana Cardoso 
    public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
    {
        if (id == null)
        {
            return NotFound();
        }
        var tbPaciente = await _context.TbPacientes
            .Include(t => t.IdCidadeNavigation)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.IdPaciente == id);
        if (tbPaciente == null)
        {
            return NotFound();
        }
        if (saveChangesError.GetValueOrDefault())
        {
            ViewData["ErrorMessage"] = "A exclusão falhou. Tente novamente.";
        }
        return View(tbPaciente);
    }

    // POST: TbPaciente/Delete/5
    //Adriana Cardoso 
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var tbPaciente = await _context.TbPacientes.FindAsync(id);
        if (tbPaciente == null)
        {
            return RedirectToAction(nameof(Index));
        }
        try
        {
            var medicoPaciente = _context.TbMedicoPacientes.SingleOrDefault(x => x.IdPaciente == id) ?? throw new Exception("Paciente não encontrado");
            _context.TbMedicoPacientes.Remove(medicoPaciente);
            _context.TbPacientes.Remove(tbPaciente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException /*ex*/)
        {
            return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
        }
    }
}
