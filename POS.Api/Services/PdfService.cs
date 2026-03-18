using POS.Api.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace POS.Api.Services
{
    public class PdfService : IPdfService
    {
        public PdfService()
        {
            // License configured in Program.cs
        }

        public byte[] GenerateDailySalesReport(List<Order> orders, string branchName, DateTime date)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header().Element(header => ComposeDailyReportHeader(header, branchName, date));
                    page.Content().Element(content => ComposeDailyReportContent(content, orders));
                    page.Footer().Element(ComposeDailyReportFooter);
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateReceipt(Order order, string branchName)
        {
            // 80mm-wide thermal receipt style
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ContinuousSize(80, Unit.Millimetre);
                    page.Margin(5, Unit.Millimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Consolas"));

                    page.Header().Element(header => ComposeReceiptHeader(header, branchName, order));
                    page.Content().Element(content => ComposeReceiptContent(content, order));
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeDailyReportHeader(IContainer container, string branchName, DateTime date)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text($"Daily Sales Report - {branchName}").FontSize(20).SemiBold();
                    column.Item().Text($"Date: {date:yyyy-MM-dd}");
                });
            });
        }

        private void ComposeDailyReportContent(IContainer container, List<Order> orders)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Order #").SemiBold();
                        header.Cell().Text("Time").SemiBold();
                        header.Cell().Text("Cashier").SemiBold();
                        header.Cell().Text("Items").SemiBold();
                        header.Cell().Text("Total").SemiBold();
                        header.Cell().Text("Payment").SemiBold();
                    });

                    decimal totalRevenue = 0;

                    foreach (var order in orders)
                    {
                        table.Cell().Text(order.Id.ToString());
                        table.Cell().Text(order.CreatedAt.ToString("HH:mm"));
                        table.Cell().Text(order.User?.FullName ?? "Unknown");
                        table.Cell().Text(order.Items.Sum(i => i.Quantity).ToString());
                        table.Cell().Text(order.TotalAmount.ToString("C2", new System.Globalization.CultureInfo("en-US")).Replace("$", "LKR "));
                        table.Cell().Text(order.PaymentMode.ToString());

                        totalRevenue += order.TotalAmount;
                    }

                    // Summary row at bottom
                    table.Cell().ColumnSpan(6).PaddingTop(10).BorderTop(1).BorderColor(Colors.Grey.Lighten1);
                    table.Cell().ColumnSpan(4).Text("Summary").SemiBold().AlignRight();
                    table.Cell().ColumnSpan(2).Text($"{orders.Count} Orders | Total: LKR {totalRevenue:N2}").SemiBold().AlignRight();
                });
            });
        }

        private void ComposeDailyReportFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" of ");
                x.TotalPages();
            });
        }

        private void ComposeReceiptHeader(IContainer container, string branchName, Order order)
        {
            container.Column(column =>
            {
                column.Item().AlignCenter().Text(branchName).FontSize(14).SemiBold();
                column.Item().AlignCenter().Text($"Order #{order.Id}");
                column.Item().AlignCenter().Text(order.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
                column.Item().PaddingBottom(5).LineHorizontal(1);
            });
        }

        private void ComposeReceiptContent(IContainer container, Order order)
        {
            container.Column(column =>
            {
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                    });

                    foreach (var item in order.Items)
                    {
                        table.Cell().Text(item.Product.Name);
                        table.Cell().AlignCenter().Text($"x{item.Quantity}");
                        table.Cell().AlignRight().Text(((item.UnitPrice * item.Quantity) - item.DiscountAmount).ToString("N2"));
                    }
                });

                column.Item().PaddingVertical(5).LineHorizontal(1);
                
                column.Item().Row(r =>
                {
                    r.RelativeItem().Text("Subtotal:");
                    r.RelativeItem().AlignRight().Text((order.TotalAmount + order.DiscountAmount).ToString("N2"));
                });

                if (order.DiscountAmount > 0)
                {
                    column.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Discount:");
                        r.RelativeItem().AlignRight().Text("-" + order.DiscountAmount.ToString("N2"));
                    });
                }

                column.Item().Row(r =>
                {
                    r.RelativeItem().Text("Total (LKR):").SemiBold();
                    r.RelativeItem().AlignRight().Text(order.TotalAmount.ToString("N2")).SemiBold();
                });

                column.Item().PaddingVertical(5).LineHorizontal(1);

                if (order.PointsEarned > 0)
                {
                    column.Item().AlignCenter().Text($"Points Earned: {order.PointsEarned}");
                }
                
                if (order.PointsRedeemed > 0)
                {
                    column.Item().AlignCenter().Text($"Points Redeemed: {order.PointsRedeemed}");
                }

                column.Item().PaddingTop(10).AlignCenter().Text("Thank you!").FontSize(10).SemiBold();
            });
        }
    }
}
