using AdvisorDashboardApp.Data;
using AdvisorDashboardApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdvisorDashboardApp.Controllers;

public class AdvisorsController : Controller
{
    private readonly AppDbContext _context;

    public AdvisorsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var advisors = await _context.Advisors
            .OrderBy(x => x.Name)
            .ToListAsync();

        return View(advisors);
    }

    public IActionResult Create()
    {
        return View(new Advisor());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Advisor advisor)
    {
        if (!ModelState.IsValid)
            return View(advisor);

        _context.Advisors.Add(advisor);
        await _context.SaveChangesAsync();

        TempData["Success"] = "A tanácsadó sikeresen felvéve.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var advisor = await _context.Advisors.FirstOrDefaultAsync(x => x.Id == id);
        if (advisor == null)
            return NotFound();

        return View(advisor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Advisor advisor)
    {
        if (id != advisor.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(advisor);

        var existingAdvisor = await _context.Advisors.FirstOrDefaultAsync(x => x.Id == id);
        if (existingAdvisor == null)
            return NotFound();

        existingAdvisor.Name = advisor.Name;
        existingAdvisor.Phone = advisor.Phone;
        existingAdvisor.Email = advisor.Email;
        existingAdvisor.IsActive = advisor.IsActive;

        await _context.SaveChangesAsync();

        TempData["Success"] = "A tanácsadó adatai sikeresen módosítva lettek.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var advisor = await _context.Advisors.FirstOrDefaultAsync(x => x.Id == id);
        if (advisor == null)
            return NotFound();

        return View(advisor);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var advisor = await _context.Advisors.FindAsync(id);
        if (advisor == null)
            return NotFound();

        _context.Advisors.Remove(advisor);
        await _context.SaveChangesAsync();

        TempData["Success"] = "A tanácsadó törölve lett.";
        return RedirectToAction(nameof(Index));
    }
}