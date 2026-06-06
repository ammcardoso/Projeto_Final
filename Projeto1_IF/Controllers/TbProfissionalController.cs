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
            var db_IFContext = _context.TbProfissionals.Include(t => t.IdCidadeNavigation).Include(t => t.IdContratoNavigation).Include(t => t.IdTipoAcessoNavigation);
            return View(await db_IFContext.ToListAsync());
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

            return View(tbProfissional);
        }

        // GET: TbProfissional/Create
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

            return View(tbProfissional);
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
