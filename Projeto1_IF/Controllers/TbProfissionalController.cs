using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projeto1_IF.Models;

namespace Projeto1_IF.Controllers;


[Authorize(Roles = "GerenteGeral,GerenteNutricionista,GerenteMedico,Medico,Nutricionista")]
public class TbProfissionalController(db_IFContext context) : Controller
{
    private readonly db_IFContext _context = context;

    // GET: TbProfissional
    [Authorize(Roles = "GerenteGeral,GerenteNutricionista,GerenteMedico")]
    public async Task<IActionResult> Index()
    {
        string email = User.Identity!.Name!;
        AspNetUser user = await _context.AspNetUsers.Include(x => x.Roles).SingleOrDefaultAsync(u => u.Email == email) ?? throw new Exception($"[ ERROR ] - Usuário {email} não encontrado");
        Role role = user.Roles.Select(x => Enum.Parse<Role>(x.Name)).SingleOrDefault();

        var query = _context.TbProfissionals
                .Include(t => t.IdCidadeNavigation)
                .Include(t => t.IdContratoNavigation)
                .ThenInclude(t => t.IdPlanoNavigation)
                .Include(t => t.IdTipoAcessoNavigation)
                .AsNoTracking()
                .AsQueryable();

        return role switch
        {
            Role.GerenteMedico => View(await query.Where(x => x.Especialidade.Equals("Medico")).ToListAsync()),
            Role.GerenteNutricionista => View(await query.Where(x => x.Especialidade.Equals("Nutricionista")).ToListAsync()),
            Role.GerenteGeral => View(await query.ToListAsync()),
            _ => throw new Exception($"[ ERROR ] - Role {role} é inválido do usuário: {email}"),
        };
    }

    // GET: TbProfissional/Details/5
    [Authorize(Roles = "GerenteGeral,GerenteNutricionista,GerenteMedico,Medico,Nutricionista")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return RedirectToAction("Error", "Home");
        }
        else if (id == 0)
        {
            string email = User.Identity!.Name!;
            AspNetUser user = await _context.AspNetUsers.Include(x => x.Roles).SingleOrDefaultAsync(u => u.Email == email) ?? throw new Exception($"[ ERROR ] - Usuário {email} não encontrado");
            TbProfissional tbProfissional2 = await _context.TbProfissionals.SingleOrDefaultAsync(x => x.IdUser == user.Id) ?? throw new Exception($"[ ERROR ] - Usuário {email} não encontrado");

            id = tbProfissional2.IdProfissional;
        }

        var tbProfissional = await _context.TbProfissionals
            .Include(t => t.IdCidadeNavigation)
            .Include(t => t.IdContratoNavigation)
            .ThenInclude(t => t.IdPlanoNavigation)
            .Include(t => t.IdTipoAcessoNavigation)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.IdProfissional == id);
        if (tbProfissional == null)
        {
            return NotFound();
        }

        return View(tbProfissional);
    }

    // GET: TbProfissional/Create
    [Authorize(Roles = "GerenteGeral,GerenteNutricionista,GerenteMedico")]
    public IActionResult Create()
    {
        ViewData["IdCidade"] = new SelectList(_context.TbCidades, "IdCidade", "Nome");
        ViewData["IdPlano"] = new SelectList(_context.TbPlanos, "IdPlano", "Nome");
        ViewData["IdTipoAcesso"] = new SelectList(_context.TbTipoAcessos, "IdTipoAcesso", "Nome");
        return View();
    }

    // POST: TbProfissional/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "GerenteGeral,GerenteNutricionista,GerenteMedico")]
    public async Task<IActionResult> Create([Bind("IdTipoProfissional,IdTipoAcesso,IdCidade,IdUser,Nome,Cpf,CrmCrn,Especialidade,Logradouro,Numero,Bairro,Cep,Cidade,Estado,Ddd1,Ddd2,Telefone1,Telefone2,Salario")] TbProfissional tbProfissional, [Bind("IdPlano")] TbContrato IdContratoNavigation)
    {
        try
        {
            ModelState.Remove("IdUser");
            ModelState.Remove("IdContrato");
            if (ModelState.IsValid)
            {
                IdContratoNavigation.DataInicio = DateTime.UtcNow;
                IdContratoNavigation.DataFim = IdContratoNavigation.DataInicio.Value.AddMonths(1);
                _context.Add(IdContratoNavigation);
                await _context.SaveChangesAsync();

                var userManager = HttpContext.RequestServices.GetService<UserManager<IdentityUser>>();
                if (userManager == null)
                    return NotFound();

                var email = User.Identity?.Name;
                if (string.IsNullOrEmpty(email))
                    return NotFound();

                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                    return NotFound();

                tbProfissional.IdUser = user.Id;
                tbProfissional.IdContrato = IdContratoNavigation.IdContrato;

                _context.Add(tbProfissional);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError(string.Empty, $"Ocorreu um erro ao criar o profissional: {ex.Message}");
        }
        ViewData["IdCidade"] = new SelectList(_context.TbCidades, "IdCidade", "Nome", tbProfissional.IdCidade);
        ViewData["IdPlano"] = new SelectList(_context.TbPlanos, "IdPlano", "Nome", IdContratoNavigation.IdPlano);
        ViewData["IdTipoAcesso"] = new SelectList(_context.TbTipoAcessos, "IdTipoAcesso", "Nome", tbProfissional.IdTipoAcesso);
        return View(tbProfissional);
    }

    // GET: TbProfissional/Edit/5
    [Authorize(Roles = "GerenteGeral,GerenteNutricionista,GerenteMedico,Medico,Nutricionista")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return RedirectToAction("Error", "Home");
        }

        var tbProfissional = await _context.TbProfissionals.Include(t => t.IdContratoNavigation).ThenInclude(t => t.IdPlanoNavigation).FirstOrDefaultAsync(s => s.IdProfissional == id);
        if (tbProfissional == null)
        {
            return NotFound();
        }

        if (tbProfissional.Especialidade.Equals("Medico"))
        {
            ViewData["IdContrato"] = new SelectList(_context.TbPlanos, "IdPlano", "Nome", tbProfissional.IdContratoNavigation.IdPlano).Where(x => x.Text.StartsWith("Medico"));
        }
        else
        {
            ViewData["IdContrato"] = new SelectList(_context.TbPlanos, "IdPlano", "Nome", tbProfissional.IdContratoNavigation.IdPlano).Where(x => x.Text.StartsWith("Nutricional"));
        }
        

        ViewData["IdCidade"] = new SelectList(_context.TbCidades, "IdCidade", "Nome", tbProfissional.IdCidade);
        ViewData["IdTipoAcesso"] = new SelectList(_context.TbTipoAcessos, "IdTipoAcesso", "Nome", tbProfissional.IdTipoAcesso);

        return View(tbProfissional);
    }

    // POST: TbProfissional/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = "GerenteGeral,GerenteNutricionista,GerenteMedico,Medico,Nutricionista")]
    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tbProfissional = await _context.TbProfissionals.Include(t => t.IdContratoNavigation).ThenInclude(t => t.IdPlanoNavigation).FirstOrDefaultAsync(s => s.IdProfissional == id);
        if (tbProfissional == null)
        {
            return NotFound();
        }
        if (await TryUpdateModelAsync<TbProfissional>(
            tbProfissional,
            "",
            s => s.IdContrato,
            s => s.IdProfissional,
            s => s.IdTipoAcesso,
            s => s.IdCidade,
            s => s.Nome,
            s => s.Cpf,
            s => s.CrmCrn,
            s => s.Especialidade,
            s => s.Logradouro,
            s => s.Numero,
            s => s.Bairro,
            s => s.Cep,
            s => s.Cidade,
            s => s.Estado,
            s => s.Ddd1,
            s => s.Ddd2,
            s => s.Telefone1,
            s => s.Telefone2,
            s => s.Salario))
        {
            try
            {
                await _context.SaveChangesAsync();
                
                if (User.IsInRole("Nutricionista") || User.IsInRole("Medico"))
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {

                return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("",
                    "Unable to save changes. " +
                    "Try again, and if the problem persists, " +
                    "see your system administrator." + ex.ToString());
            }
        }
        ViewData["IdCidade"] = new SelectList(_context.TbCidades, "IdCidade", "IdCidade", tbProfissional.IdCidade);
        ViewData["IdContrato"] = new SelectList(_context.TbPlanos, "IdPlano", "Nome", tbProfissional.IdContratoNavigation.IdPlano);
        ViewData["IdTipoAcesso"] = new SelectList(_context.TbTipoAcessos, "IdTipoAcesso", "Nome", tbProfissional.IdTipoAcesso);

        return View(tbProfissional);
    }

    // GET: TbProfissional/Delete/5
    [Authorize(Roles = "GerenteGeral,GerenteNutricionista,GerenteMedico")]
    public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tbProfissional = await _context.TbProfissionals
            .Include(t => t.IdCidadeNavigation)
            .Include(t => t.IdContratoNavigation)
            .ThenInclude(t => t.IdPlanoNavigation)
            .Include(t => t.IdTipoAcessoNavigation)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.IdProfissional == id);
        if (tbProfissional == null)
        {
            return NotFound();
        }

        bool hasPatient = await _context.TbMedicoPacientes.AnyAsync(mp => mp.IdProfissional == id);

        if (hasPatient)
        {
            ModelState.AddModelError(string.Empty, "Não é possível excluir o profissional pois possui paciente.");
        }

        return View(tbProfissional);
    }

    // POST: TbProfissional/Delete/5
    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "GerenteGeral,GerenteNutricionista,GerenteMedico")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var tbProfissional = await _context.TbProfissionals
            .Include(t => t.IdCidadeNavigation)
            .Include(t => t.IdContratoNavigation)
            .Include(t => t.IdTipoAcessoNavigation)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.IdProfissional == id);

        if (tbProfissional == null)
        {
            return RedirectToAction(nameof(Index));
        }

        bool hasPatient = await _context.TbMedicoPacientes.AnyAsync(mp => mp.IdProfissional == id);

        if (hasPatient)
        {
            ModelState.AddModelError(string.Empty, "Não é possível excluir o profissional pois possui paciente.");
            return View(tbProfissional);
        }

        try
        {
            var profissionalUser = await _context.AspNetUsers.FindAsync(tbProfissional.IdUser) ?? throw new Exception("Erro na exclusão do profissional");
            
            _context.TbProfissionals.Remove(tbProfissional);
            await _context.SaveChangesAsync();
            _context.AspNetUsers.Remove(profissionalUser);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
        catch (DbUpdateException /*ex*/)
        {
            return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
        }

    }
}
