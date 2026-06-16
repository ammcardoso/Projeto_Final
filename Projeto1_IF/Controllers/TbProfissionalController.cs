using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projeto1_IF.Models;

namespace Projeto1_IF.Controllers
{
    [Authorize]
    public class TbProfissionalController : Controller
    {
        private readonly db_IFContext _context;

        public TbProfissionalController(db_IFContext context)
        {
            _context = context;
        }

        // GET: TbProfissional
        public async Task<IActionResult> Index()
        {
            string email = User.Identity!.Name!;

            AspNetUser user = _context.AspNetUsers.Include(x => x.Roles).Single(u => u.Email == email)!;

            var roles = user.Roles.Select(r => r.Name).ToList();

            if (roles.Any(r => r == "GerenteMedico"))
            {
                // Build base query with includes
                var query = _context.TbProfissionals
                    .Include(t => t.IdCidadeNavigation)
                    .Include(t => t.IdContratoNavigation)
                    .Include(t => t.IdTipoAcessoNavigation)
                    .AsNoTracking()
                   
                    .AsQueryable();

                var result = (await query.ToListAsync()).Where(r => r.Especialidade.Equals("Medico")).ToList();
                return View(result);
            } else if (roles.Any(r => r == "GerenteNutricionista"))
                {
               


                // Build base query with includes
                var query = _context.TbProfissionals
                    .Include(t => t.IdCidadeNavigation)
                    .Include(t => t.IdContratoNavigation)
                    .Include(t => t.IdTipoAcessoNavigation)
                    .AsNoTracking()

                    .AsQueryable();

                var result = (await query.ToListAsync()).Where(r => r.Especialidade.Equals("Nutricionista")).ToList();
                return View(result);
            }
                // Build base query with includes
                var query2 = _context.TbProfissionals
                    .Include(t => t.IdCidadeNavigation)
                    .Include(t => t.IdContratoNavigation)
                    .Include(t => t.IdTipoAcessoNavigation)
                    .AsNoTracking()
                    .AsQueryable();


                // Default: return all
                return View(await query2.ToListAsync());


            
        }

        // GET: TbProfissional/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbProfissional = await _context.TbProfissionals
                .Include(t => t.IdCidadeNavigation)
                .Include(t => t.IdContratoNavigation)
                .Include(t => t.IdTipoAcessoNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdProfissional == id);
            if (tbProfissional == null)
            {
                return NotFound();
            }
            // Allow access to owners or managers within their scope
            var email = User.Identity?.Name;
            // Owners (Medico or Nutricionista) can view their own record
            if (User.IsInRole("Medico") || User.IsInRole("Nutricionista"))
            {
                if (string.IsNullOrEmpty(email))
                    return NotFound();
                var userManager = HttpContext.RequestServices.GetService<UserManager<IdentityUser>>();
                if (userManager == null)
                    return NotFound();
                var user = await userManager.FindByEmailAsync(email);
                if (user == null || tbProfissional.IdUser != user.Id)
                    return NotFound();
                return View(tbProfissional);
            }

            // Managers
            if (User.IsInRole("GerenteGeral"))
            {
                return View(tbProfissional);
            }
            if (User.IsInRole("GerenteMedico") && tbProfissional.IdTipoProfissional == 1)
            {
                return View(tbProfissional);
            }
            if (User.IsInRole("GerenteNutricionista") && tbProfissional.IdTipoProfissional == 2)
            {
                return View(tbProfissional);
            }

            return Forbid();
        }

        // GET: TbProfissional/Create
        public IActionResult Create()
        {
            // Professionals should not create other professionals; allow access only to admins or managers
            if (User.IsInRole("Medico") || User.IsInRole("Nutricionista") || User.IsInRole("GerenteMedico") || User.IsInRole("GerenteNutricionista") || User.IsInRole("GerenteGeral"))
            {
                // managers and professionals cannot create professionals here
                return Forbid();
            }

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
        public async Task<IActionResult> Create([Bind("IdTipoProfissional,IdTipoAcesso,IdCidade,IdUser,Nome,Cpf,CrmCrn,Especialidade,Logradouro,Numero,Bairro,Cep,Cidade,Estado,Ddd1,Ddd2,Telefone1,Telefone2,Salario")] TbProfissional tbProfissional, [Bind("IdPlano")] TbContrato IdContratoNavigation)
        {
            try
            {
                if (User.IsInRole("Medico") || User.IsInRole("Nutricionista"))
                {
                    // Prevent professionals from creating other professionals
                    return Forbid();
                }
                ModelState.Remove("IdUser");
                ModelState.Remove("IdContrato");
                if (ModelState.IsValid)
                {
                    IdContratoNavigation.DataInicio = DateTime.UtcNow;
                    IdContratoNavigation.DataFim = IdContratoNavigation.DataInicio.Value.AddMonths(1);
                    _context.Add(IdContratoNavigation);
                    await _context.SaveChangesAsync();

                    var userManager = HttpContext.RequestServices.GetService<UserManager<IdentityUser>>();
                    // Logica diferente realizada para obter o email do usuário autenticado
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
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var tbProfissional = await _context.TbProfissionals.Include(t => t.IdContratoNavigation).FirstOrDefaultAsync(s => s.IdProfissional == id);
            if (tbProfissional == null)
            {
                return NotFound();
            }
            // Ensure only owner or admin can edit
            var email = User.Identity?.Name;
            if (User.IsInRole("Medico") || User.IsInRole("Nutricionista"))
            {
                if (string.IsNullOrEmpty(email))
                    return NotFound();
                var userManager = HttpContext.RequestServices.GetService<UserManager<IdentityUser>>();
                if (userManager == null)
                    return NotFound();
                var user = await userManager.FindByEmailAsync(email);
                if (user == null || tbProfissional.IdUser != user.Id)
                    return NotFound();
            }
            else if (User.IsInRole("GerenteGeral"))
            {
                // ok
            }
            else if (User.IsInRole("GerenteMedico") && tbProfissional.IdTipoProfissional == 1)
            {
                // ok
            }
            else if (User.IsInRole("GerenteNutricionista") && tbProfissional.IdTipoProfissional == 2)
            {
                // ok
            }
            else
            {
                return Forbid();
            }

            ViewData["IdCidade"] = new SelectList(_context.TbCidades, "IdCidade", "IdCidade", tbProfissional.IdCidade);
            ViewData["IdContrato"] = new SelectList(_context.TbPlanos, "IdPlano", "Nome", tbProfissional.IdContratoNavigation.IdPlano);
            ViewData["IdTipoAcesso"] = new SelectList(_context.TbTipoAcessos, "IdTipoAcesso", "Nome", tbProfissional.IdTipoAcesso);
            return View(tbProfissional);
        }

        // POST: TbProfissional/Edit/5
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
            var tbProfissional = await _context.TbProfissionals.Include(t => t.IdContratoNavigation).FirstOrDefaultAsync(s => s.IdProfissional == id);
            if (tbProfissional == null) 
            {
                return NotFound();
            }
            // Ensure only owner or admin can update
            var email = User.Identity?.Name;
            if (User.IsInRole("Medico") || User.IsInRole("Nutricionista"))
            {
                if (string.IsNullOrEmpty(email))
                    return NotFound();
                var userManager = HttpContext.RequestServices.GetService<UserManager<IdentityUser>>();
                if (userManager == null)
                    return NotFound();
                var user = await userManager.FindByEmailAsync(email);
                if (user == null || tbProfissional.IdUser != user.Id)
                    return NotFound();
            }
            else if (User.IsInRole("GerenteGeral"))
            {
                // ok
            }
            else if (User.IsInRole("GerenteMedico") && tbProfissional.IdTipoProfissional == 1)
            {
                // ok
            }
            else if (User.IsInRole("GerenteNutricionista") && tbProfissional.IdTipoProfissional == 2)
            {
                // ok
            }
            else
            {
                return Forbid();
            }

            // Allow CPF update for admins and manager roles; professionals cannot change CPF
            bool canEditCpf = User.IsInRole("GerenteGeral") || User.IsInRole("GerenteMedico") || User.IsInRole("GerenteNutricionista");

            if (canEditCpf)
            {
                if (await TryUpdateModelAsync<TbProfissional>(
                    tbProfissional,
                    "",
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
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateException ex)
                    {
                        ModelState.AddModelError("",
                            "Unable to save changes. " +
                            "Try again, and if the problem persists, " +
                            "see your system administrator." + ex.ToString());
                    }
                }
            }
            else
            {
                if (await TryUpdateModelAsync<TbProfissional>(
                    tbProfissional,
                    "",
                    s => s.IdProfissional,
                    s => s.IdTipoAcesso,
                    s => s.IdCidade,
                    s => s.Nome,
                    // CPF not included for professionals
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
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateException ex)
                    {
                        ModelState.AddModelError("",
                            "Unable to save changes. " +
                            "Try again, and if the problem persists, " +
                            "see your system administrator." + ex.ToString());
                    }
                }
            }
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbProfissional = await _context.TbProfissionals
                .Include(t => t.IdCidadeNavigation)
                .Include(t => t.IdContratoNavigation)
                .Include(t => t.IdTipoAcessoNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdProfissional == id);
            if (tbProfissional == null)
            {
                return NotFound();
            }

            // Professionals are not allowed to delete
            if (User.IsInRole("Medico") || User.IsInRole("Nutricionista"))
            {
                return Forbid();
            }

            // Managers within scope can view delete page
            if (User.IsInRole("GerenteGeral"))
            {
                return View(tbProfissional);
            }
            if (User.IsInRole("GerenteMedico") && tbProfissional.IdTipoProfissional == 1)
            {
                return View(tbProfissional);
            }
            if (User.IsInRole("GerenteNutricionista") && tbProfissional.IdTipoProfissional == 2)
            {
                return View(tbProfissional);
            }

            return Forbid();
        }

        // POST: TbProfissional/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tbProfissional = await _context.TbProfissionals.FindAsync(id);
            if (tbProfissional == null)
            {
                return RedirectToAction(nameof(Index));
            }
            try
            {
                // Only managers within scope can delete; professionals cannot
                if (User.IsInRole("Medico") || User.IsInRole("Nutricionista"))
                {
                    return Forbid();
                }
                if (User.IsInRole("GerenteGeral"))
                {
                    // ok
                }
                else if (User.IsInRole("GerenteMedico") && tbProfissional.IdTipoProfissional == 1)
                {
                    // ok
                }
                else if (User.IsInRole("GerenteNutricionista") && tbProfissional.IdTipoProfissional == 2)
                {
                    // ok
                }
                else
                {
                    return Forbid();
                }

                // Check again before deleting
                var hasPatientsConfirmed = await _context.TbMedicoPacientes.AnyAsync(mp => mp.IdProfissional == tbProfissional.IdProfissional);
                if (hasPatientsConfirmed)
                {
                    ModelState.AddModelError("", "Cannot delete a professional who has patients assigned.");
                    return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
                }

                _context.TbProfissionals.Remove(tbProfissional);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }
            catch (DbUpdateException /*ex*/)
            {
                 return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
            }

        }
    }
}
