using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimplePOS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ClosedXML.Excel;
using System.Diagnostics;
using System.IO;

namespace SimplePOS.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _cajaName = "Caja nro 1";

        [ObservableProperty]
        private decimal _initialCash = 0.00m;

        [ObservableProperty]
        private decimal _totalSales = 0.00m; 

        [ObservableProperty]
        private decimal _currentTotal = 0.00m;

        public ObservableCollection<Product> AvailableProducts { get; } = new();
        public ObservableCollection<TicketItem> Cart { get; } = new();
        public ObservableCollection<Withdrawal> Withdrawals { get; } = new();

        // Track items sold in current session
        private List<TicketItem> _sessionItems = new();

        public MainViewModel()
        {
            LoadConfig();
            LoadSampleProducts();
            try {
                using var db = new PosDbContext();
                db.Database.EnsureCreated();
            } catch { /* Silent if SQL Server not ready during dev */ }
        }

        private void LoadConfig()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
            if (File.Exists(configPath))
            {
                var lines = File.ReadAllLines(configPath);
                var cajaLine = lines.FirstOrDefault(l => l.StartsWith("CajaNumber="));
                if (cajaLine != null)
                {
                    string num = cajaLine.Split('=')[1].Trim();
                    CajaName = $"Caja nro {num}";
                }
            }
            else
            {
                // Create default config if it doesn't exist
                File.WriteAllText(configPath, "[General]\nCajaNumber=1");
                CajaName = "Caja nro 1";
            }
        }

        private void LoadSampleProducts()
        {
            try
            {
                using var db = new PosDbContext();
                var dbProducts = db.Products.ToList();
                
                if (dbProducts.Any())
                {
                    foreach (var p in dbProducts) AvailableProducts.Add(p);
                }
                else
                {
                    // Default products if DB is empty
                    var defaults = new List<Product>
                    {
                        new Product { Name = "GASEOSA X 500CC", Price = 3000, Color = "#7dd3fc" },
                        new Product { Name = "CERVEZA", Price = 2500, Color = "#fdba74" },
                        new Product { Name = "CHORIPAN", Price = 4000, Color = "#6ee7b7" },
                        new Product { Name = "HAMBURGUESA", Price = 4000, Color = "#fda4af" },
                        new Product { Name = "AGUA X 500CC", Price = 2000, Color = "#93c5fd" },
                        new Product { Name = "PRITIADO", Price = 10000, Color = "#c4b5fd" },
                        new Product { Name = "CERVEZA LATA", Price = 3000, Color = "#fde047" },
                        new Product { Name = "HIELO", Price = 2000, Color = "#e2e8f0" },
                        new Product { Name = "CONO PAPAS", Price = 4000, Color = "#f9a8d4" },
                        new Product { Name = "FERNET CON COCA", Price = 13000, Color = "#d1d5db" }
                    };
                    db.Products.AddRange(defaults);
                    db.SaveChanges();
                    foreach (var p in defaults) AvailableProducts.Add(p);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar productos: {ex.Message}");
            }
        }

        [RelayCommand]
        private void CreateProduct()
        {
            var window = new SimplePOS.Views.AddProductWindow();
            if (window.ShowDialog() == true && window.NewProduct != null)
            {
                try
                {
                    using var db = new PosDbContext();
                    db.Products.Add(window.NewProduct);
                    db.SaveChanges();
                    AvailableProducts.Add(window.NewProduct);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error al guardar: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private void AddBulkToCart(Product product)
        {
            var window = new SimplePOS.Views.QuantityInputWindow(product.Name);
            if (window.ShowDialog() == true)
            {
                AddUnitsToCart(product, window.Quantity);
            }
        }

        private void AddUnitsToCart(Product product, int quantity)
        {
            var existingItem = Cart.FirstOrDefault(i => i.ProductName == product.Name);
            
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                OnPropertyChanged(nameof(Cart));
            }
            else
            {
                Cart.Add(new TicketItem 
                { 
                    ProductName = product.Name, 
                    Price = product.Price, 
                    Quantity = quantity 
                });
            }

            CalculateTotal();
        }

        [RelayCommand]
        private void AddToCart(Product product)
        {
            AddUnitsToCart(product, 1);
        }

        private void CalculateTotal()
        {
            CurrentTotal = Cart.Sum(item => item.Price * item.Quantity);
        }

        [RelayCommand]
        private void RemoveFromCart(TicketItem item)
        {
            Cart.Remove(item);
            CalculateTotal();
        }

        [RelayCommand]
        private void ConfirmSale()
        {
            if (Cart.Count == 0) return;

            // Step 1: Select Payment Method
            var paymentWindow = new SimplePOS.Views.PaymentMethodWindow();
            paymentWindow.Owner = System.Windows.Application.Current.MainWindow;
            if (paymentWindow.ShowDialog() != true) return; // User cancelled

            string selectedMethod = paymentWindow.SelectedMethod ?? "Efectivo";

            try
            {
                PrintTicket();
                
                // Track items for session
                foreach (var item in Cart)
                {
                    // For the detail, we group by product name only
                    var sessionItem = _sessionItems.FirstOrDefault(i => i.ProductName == item.ProductName);
                    if (sessionItem != null)
                    {
                        sessionItem.Quantity += item.Quantity;
                    }
                    else
                    {
                        _sessionItems.Add(new TicketItem 
                        { 
                            ProductName = item.ProductName, 
                            Price = item.Price, 
                            Quantity = item.Quantity,
                            PaymentMethod = selectedMethod // We store the method used in this sale
                        });
                    }

                    // We also need to track the payment totals separately
                    // Let's use a hidden structure or just recalculate from a new list of all sales
                    _allSalesRecords.Add(new TicketItem { Price = item.Price, Quantity = item.Quantity, PaymentMethod = selectedMethod });
                }

                TotalSales += CurrentTotal;
                Cart.Clear();
                CalculateTotal();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al imprimir: {ex.Message}");
            }
        }

        private List<TicketItem> _allSalesRecords = new();

        [RelayCommand]
        private void CerrarCaja()
        {
            if (_sessionItems.Count == 0 && Withdrawals.Count == 0) return;

            try
            {
                PrintClosingTicket();
                GenerateExcelReport();

                // Save to Database
                using (var db = new PosDbContext())
                {
                    db.CashClosings.Add(new CashClosing 
                    { 
                        Date = DateTime.Now, 
                        TotalSales = TotalSales 
                    });
                    db.SaveChanges();
                }

                // Reset session
                _sessionItems.Clear();
                _allSalesRecords.Clear();
                Withdrawals.Clear();
                TotalSales = 0;
                InitialCash = 0; // Reset for next shift? Usually yes.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cerrar caja: {ex.Message}");
            }
        }

        private void PrintClosingTicket()
        {
            using var pd = new System.Drawing.Printing.PrintDocument();
            pd.PrintPage += (sender, e) =>
            {
                float yPos = 10;
                var font = new System.Drawing.Font("Consolas", 8);
                var boldFont = new System.Drawing.Font("Consolas", 9, System.Drawing.FontStyle.Bold);
                var graphics = e.Graphics;

                if (graphics == null) return;

                // Header
                graphics.DrawString($"LISTADO DE VENTAS DE {CajaName.ToUpper()}", boldFont, System.Drawing.Brushes.Black, 5, yPos);
                yPos += 20;
                graphics.DrawString("CARNAVALES 2026", boldFont, System.Drawing.Brushes.Black, 5, yPos);
                yPos += 30;

                // Products detail
                foreach (var item in _sessionItems)
                {
                    graphics.DrawString(item.ProductName.PadRight(25).Substring(0, 18), font, System.Drawing.Brushes.Black, 5, yPos);
                    graphics.DrawString(item.Quantity.ToString().PadLeft(3), font, System.Drawing.Brushes.Black, 150, yPos);
                    graphics.DrawString(item.Subtotal.ToString("F2").PadLeft(12), font, System.Drawing.Brushes.Black, 190, yPos);
                    yPos += 15;
                }

                graphics.DrawString("------------------------------------------", font, System.Drawing.Brushes.Black, 5, yPos);
                yPos += 20;

                // Totals
                graphics.DrawString("TOTAL VENTAS", boldFont, System.Drawing.Brushes.Black, 5, yPos);
                graphics.DrawString(TotalSales.ToString("F2").PadLeft(12), boldFont, System.Drawing.Brushes.Black, 190, yPos);
                yPos += 25;

                graphics.DrawString("CAMBIO INICIAL", font, System.Drawing.Brushes.Black, 5, yPos);
                graphics.DrawString(InitialCash.ToString("F2").PadLeft(12), font, System.Drawing.Brushes.Black, 190, yPos);
                yPos += 25;

                // Withdrawals
                foreach (var w in Withdrawals)
                {
                    graphics.DrawString($"RETIRO NRO° : {w.Id:D3}", font, System.Drawing.Brushes.Black, 5, yPos);
                    graphics.DrawString($"-{w.Amount:F2}".PadLeft(12), font, System.Drawing.Brushes.Black, 190, yPos);
                    yPos += 15;
                }

                yPos += 10;
                decimal totalWithdrawals = Withdrawals.Sum(w => w.Amount);
                decimal saldoTotal = TotalSales + InitialCash - totalWithdrawals;

                graphics.DrawString("SALDO TOTAL DE CAJA", boldFont, System.Drawing.Brushes.Black, 5, yPos);
                graphics.DrawString(saldoTotal.ToString("F2").PadLeft(12), boldFont, System.Drawing.Brushes.Black, 190, yPos);
                yPos += 20;
                graphics.DrawString("------------------------------------------", font, System.Drawing.Brushes.Black, 5, yPos);
                yPos += 20;

                decimal mpTotal = _allSalesRecords.Where(s => s.PaymentMethod == "Mercado Pago/Transferencia").Sum(s => s.Subtotal);
                decimal cashInHand = saldoTotal - mpTotal;

                graphics.DrawString("COMPROBANTES DIGITALES", font, System.Drawing.Brushes.Black, 5, yPos);
                graphics.DrawString(mpTotal.ToString("F2").PadLeft(12), font, System.Drawing.Brushes.Black, 190, yPos);
                yPos += 20;

                graphics.DrawString("EFECTIVO EN CAJA", font, System.Drawing.Brushes.Black, 5, yPos);
                graphics.DrawString(cashInHand.ToString("F2").PadLeft(12), font, System.Drawing.Brushes.Black, 190, yPos);
                yPos += 30;

                // Footer
                graphics.DrawString("", font, System.Drawing.Brushes.Black, 5, yPos);
            };

            pd.Print();
        }

        private void GenerateExcelReport()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Cierre de Caja");

            // Header Section
            worksheet.Cell("C1").Value = CajaName.ToUpper();
            worksheet.Range("C1:D1").Merge().Style.Fill.SetBackgroundColor(XLColor.Yellow).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Font.SetBold();
            worksheet.Cell("C2").Value = "CANT";
            worksheet.Cell("D2").Value = "IMPORTE";
            worksheet.Range("C2:D2").Style.Fill.SetBackgroundColor(XLColor.Yellow).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Font.SetBold();

            // Rows content
            int row = 3;
            int index = 1;
            foreach (var item in _sessionItems)
            {
                worksheet.Cell(row, 1).Value = index++;
                worksheet.Cell(row, 2).Value = item.ProductName;
                worksheet.Cell(row, 3).Value = item.Quantity;
                worksheet.Cell(row, 4).Value = item.Subtotal;
                worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
                row++;
            }

            // Separated Totals
            row++;
            decimal cashTotal = _allSalesRecords.Where(s => s.PaymentMethod == "Efectivo").Sum(s => s.Subtotal);
            decimal mpTotal = _allSalesRecords.Where(s => s.PaymentMethod == "Mercado Pago/Transferencia").Sum(s => s.Subtotal);

            worksheet.Cell(row, 2).Value = "TOTAL EFECTIVO";
            worksheet.Cell(row, 4).Value = cashTotal;
            worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            row++;

            worksheet.Cell(row, 2).Value = "TOTAL MERCADO PAGO";
            worksheet.Cell(row, 4).Value = mpTotal;
            worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            row++;

            // Global Footer
            row++; 
            worksheet.Cell(row, 2).Value = "TOTAL VENTAS";
            worksheet.Cell(row, 2).Style.Font.SetBold();
            
            worksheet.Cell(row, 4).Value = TotalSales;
            worksheet.Cell(row, 4).Style.Font.SetBold().NumberFormat.Format = "#,##0.00";
            worksheet.Range(row, 4, row, 4).Style.Border.SetBottomBorder(XLBorderStyleValues.Thick).Border.SetTopBorder(XLBorderStyleValues.Thin).Border.SetLeftBorder(XLBorderStyleValues.Thin).Border.SetRightBorder(XLBorderStyleValues.Thin);

            // General styling
            worksheet.Range(1, 1, row, 4).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border.SetInsideBorder(XLBorderStyleValues.Thin);
            worksheet.Columns().AdjustToContents();

            string fileName = $"Cierre_Caja_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
            workbook.SaveAs(path);

            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }

        [RelayCommand]
        private void UpdatePrices()
        {
            var window = new SimplePOS.Views.UpdatePriceWindow(this);
            window.ShowDialog();
        }

        [RelayCommand]
        private void ProcessWithdrawal()
        {
            var window = new SimplePOS.Views.WithdrawalWindow();
            if (window.ShowDialog() == true)
            {
                var withdrawal = new Withdrawal
                {
                    Id = Withdrawals.Count + 1,
                    Amount = window.Amount
                };
                
                Withdrawals.Add(withdrawal);
                PrintWithdrawalTicket(withdrawal);
            }
        }

        private void PrintWithdrawalTicket(Withdrawal withdrawal)
        {
            using var pd = new System.Drawing.Printing.PrintDocument();
            pd.PrintPage += (sender, e) =>
            {
                float yPos = 10;
                var font = new System.Drawing.Font("Consolas", 10);
                var boldFont = new System.Drawing.Font("Consolas", 12, System.Drawing.FontStyle.Bold);
                var graphics = e.Graphics;

                if (graphics == null) return;

                graphics.DrawString($"Retiro de caja N° {withdrawal.Id}", boldFont, System.Drawing.Brushes.Black, 10, yPos);
                yPos += 30;
                graphics.DrawString($"MONTO: {withdrawal.Amount:C0}", font, System.Drawing.Brushes.Black, 10, yPos);
            };

            pd.Print();
        }

        private void PrintTicket()
        {
            using var pd = new System.Drawing.Printing.PrintDocument();
            pd.PrintPage += (sender, e) =>
            {
                float yPos = 10;
                var font = new System.Drawing.Font("Consolas", 9); // Slightly smaller font for thermal 80mm
                var graphics = e.Graphics;

                if (graphics == null) return;

                foreach (var item in Cart)
                {
                    for (int i = 0; i < item.Quantity; i++)
                    {
                        // Name on the left, Price on the right
                        // Adjusting 190 to fit 80mm (total width ~280-300)
                        graphics.DrawString(item.ProductName, font, System.Drawing.Brushes.Black, 5, yPos);
                        graphics.DrawString(item.Price.ToString("C0"), font, System.Drawing.Brushes.Black, 190, yPos);
                        yPos += 18;
                    }
                }
            };

            pd.Print();
        }
    }
}
