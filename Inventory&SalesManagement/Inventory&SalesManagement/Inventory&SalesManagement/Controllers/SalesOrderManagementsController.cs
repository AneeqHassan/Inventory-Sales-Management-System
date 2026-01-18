using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Inventory_SalesManagement.Models;
using Inventory_SalesManagement.Models.Entities;
using ClosedXML.Excel;

namespace Inventory_SalesManagement.Controllers
{
    public class SalesOrderManagementsController : Controller
    {
        private readonly DatabaseContext _context;

        public SalesOrderManagementsController(DatabaseContext context)
        {
            _context = context;
        }

            // GET: Sales
            public async Task<IActionResult> Index()
            {
                // We include "Product" so the Index page can show the Name instead of just ID
                var sales = _context.SalesOrderManagements.Include(s => s.Product).OrderByDescending(s => s.OrderDate);
                return View(await sales.ToListAsync());
            }

        // GET: SalesOrderManagements/Create
        public IActionResult Create()
        {
            // ADD 'p.Barcode' to this list
            ViewBag.ProductList = _context.Products
                .Select(p => new { p.Id, p.Name, p.Price, p.StockQuantity, p.Barcode }) // <--- Added Barcode
                .ToList();

            return View(new SalesOrder());
        }

        // POST: SalesOrderManagements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SalesOrder model)
        {
            // 1. Generate a Single Invoice Number for the whole batch
            string newInvoiceId = "INV-" + DateTime.Now.ToString("yyyyMMdd-HHmm") + "-" + new Random().Next(10, 99);
            string salesPerson = User.Identity.IsAuthenticated ? User.Identity.Name : "Staff";

            // 2. Validate List is not empty
            if (model.Items == null || model.Items.Count == 0)
            {
                ModelState.AddModelError("", "Please add at least one product.");
                // Reload products for the view
                ViewBag.ProductList = _context.Products.Select(p => new { p.Id, p.Name, p.Price, p.StockQuantity }).ToList();
                return View(model);
            }

            // 3. Loop through each item and process it
            foreach (var item in model.Items)
            {
                var productInDb = await _context.Products.FindAsync(item.ProductId);

                // Skip invalid rows
                if (productInDb == null) continue;

                // Stock Check
                if (productInDb.StockQuantity < item.Quantity)
                {
                    ModelState.AddModelError("", $"Not enough stock for {productInDb.Name}. Available: {productInDb.StockQuantity}");
                    ViewBag.ProductList = _context.Products.Select(p => new { p.Id, p.Name, p.Price, p.StockQuantity }).ToList();
                    return View(model);
                }

                // Set the details
                item.OrderNumber = newInvoiceId; // Same invoice for all
                item.OrderDate = DateTime.Now;
                item.SalesPersonId = salesPerson;
                item.UnitPrice = productInDb.Price;
                item.TotalAmount = item.Quantity * productInDb.Price;
                item.Product = null; // Prevent EF from creating new products

                // Update Inventory
                productInDb.StockQuantity -= item.Quantity;

                // Add to DB Context
                _context.Add(item);
                _context.Update(productInDb);
            }

            // 4. Save All at once
            await _context.SaveChangesAsync();
            TempData["Success"] = "Sale completed successfully!";
            return RedirectToAction(nameof(Index));
        }



        // GET: SalesOrderManagements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salesOrderManagement = await _context.SalesOrderManagements
                .Include(s => s.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (salesOrderManagement == null)
            {
                return NotFound();
            }

            return View(salesOrderManagement);
        }

       
        // GET: SalesOrderManagements/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salesOrderManagement = await _context.SalesOrderManagements.FindAsync(id);
            if (salesOrderManagement == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id", salesOrderManagement.ProductId);
            return View(salesOrderManagement);
        }

        // POST: SalesOrderManagements/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderNumber,OrderDate,SalesPersonId,ProductId,Quantity,UnitPrice,TotalAmount")] SalesOrderManagement salesOrderManagement)
        {
            if (id != salesOrderManagement.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(salesOrderManagement);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SalesOrderManagementExists(salesOrderManagement.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id", salesOrderManagement.ProductId);
            return View(salesOrderManagement);
        }

        // GET: SalesOrderManagements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salesOrderManagement = await _context.SalesOrderManagements
                .Include(s => s.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (salesOrderManagement == null)
            {
                return NotFound();
            }

            return View(salesOrderManagement);
        }

        // POST: SalesOrderManagements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var salesOrderManagement = await _context.SalesOrderManagements.FindAsync(id);
            if (salesOrderManagement != null)
            {
                _context.SalesOrderManagements.Remove(salesOrderManagement);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult ExportToExcel()
        {
            var sales = _context.SalesOrderManagements.Include(s => s.Product).ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sales Report");

                // Headers
                worksheet.Cell(1, 1).Value = "Date";
                worksheet.Cell(1, 2).Value = "Invoice";
                worksheet.Cell(1, 3).Value = "Product";
                worksheet.Cell(1, 4).Value = "Quantity";
                worksheet.Cell(1, 5).Value = "Total";

                // Data
                int row = 2;
                foreach (var sale in sales)
                {
                    worksheet.Cell(row, 1).Value = sale.OrderDate;
                    worksheet.Cell(row, 2).Value = sale.OrderNumber;
                    worksheet.Cell(row, 3).Value = sale.Product?.Name;
                    worksheet.Cell(row, 4).Value = sale.Quantity;
                    worksheet.Cell(row, 5).Value = sale.TotalAmount;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SalesReport.xlsx");
                }
            }
        }

        private bool SalesOrderManagementExists(int id)
        {
            return _context.SalesOrderManagements.Any(e => e.Id == id);
        }
    }
}
