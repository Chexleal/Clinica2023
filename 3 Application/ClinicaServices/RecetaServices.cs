using ClinicaDomain;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace ClinicaServices;

public interface IRecetaServices
{
    void AddDetalleReceta(DetalleReceta detalleReceta);
    Receta Get(Guid id);
    Receta GetByConsulta(Guid id);
    void Update(Receta receta);
    void Create(Receta receta);
    List<DetalleReceta> GetAllDetalles(Guid id);
    List<Medicamento> GetAllMedicamentos();
    Guid DeleteDetalle(Guid idDetalleReceta);
    byte[] GenerarRecetaPdf(RecetaPdfData model);
}

public class RecetaServices : IRecetaServices
{
    private readonly ClinicaContext _dbContext;

    public RecetaServices(ClinicaContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void AddDetalleReceta(DetalleReceta detalleReceta)
    {
        detalleReceta.IdDetalleReceta = Guid.NewGuid();
        detalleReceta.BeforeSaveChanges();
        _dbContext.DetalleReceta.Add(detalleReceta);
        _dbContext.SaveChanges();

        InsertarMedicamento(detalleReceta.Medicamento);
    }

    private void InsertarMedicamento(string medicamento)
    {
        var existingMed = _dbContext.Medicamento.FirstOrDefault(x => x.Nombre == medicamento);
        if (existingMed is null)
        {
            _dbContext.Medicamento.Add(new Medicamento { IdMedicamento = Guid.NewGuid(), Nombre = medicamento });
            _dbContext.SaveChanges();
        }
    }

    public Receta Get(Guid id)
    {
        return _dbContext.Receta.FirstOrDefault(p => p.IdReceta == id);
    }

    public Receta GetByConsulta(Guid id)
    {
        return _dbContext.Receta.FirstOrDefault(p => p.IdConsulta == id);
    }

    public void Update(Receta receta)
    {
        receta.Descripcion ??= string.Empty;
        _dbContext.SaveChanges();
    }

    public void recetaConverter()
    {
        var builder = WebApplication.CreateBuilder();
        // builder.Services.AddSingleton(typeof(Converter(), new Synchrini));
    }

    public void Create(Receta receta)
    {
        receta.IdReceta = Guid.NewGuid();
        receta.Fecha = DateTime.Now;
        receta.Descripcion ??= string.Empty;
        _dbContext.Receta.Add(receta);
        _dbContext.SaveChanges();
    }

    public List<DetalleReceta> GetAllDetalles(Guid id)
    {
        return _dbContext.DetalleReceta.Where(x => x.IdReceta == id).ToList();
    }

    public List<Medicamento> GetAllMedicamentos()
    {
        return _dbContext.Medicamento.ToList();
    }

    public Guid DeleteDetalle(Guid idDetalleReceta)
    {
        var detalle = _dbContext.DetalleReceta.FirstOrDefault(x => x.IdDetalleReceta == idDetalleReceta);
        _dbContext.DetalleReceta.Remove(detalle);
        _dbContext.SaveChanges();

        return detalle.IdReceta;
    }

    public byte[] GenerarRecetaPdf(RecetaPdfData model)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(10, Unit.Millimetre);
                page.DefaultTextStyle(x => x.FontFamily("Roboto").FontSize(10).FontColor(Colors.Black));

                page.Header().Height(60).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().MaxHeight(50, Unit.Millimetre).Image("wwwroot/img/NewLogoOp.png").FitWidth();
                    });
                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("Horario:").Bold();
                        col.Item().Text("Lunes a Viernes 9:00-18:00");
                        col.Item().Text("Sábados 9:00-13:00");
                    });
                });

                page.Content().Column(col =>
                {
                    col.Item().PaddingTop(5).LineHorizontal(0.5f).LineColor("#004169");

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Dirección:").Bold().FontSize(11);
                            col.Item().Text("Calzada Santa Lucia Norte No. 22").FontSize(10);
                            col.Item().Text("Antigua Guatemala").FontSize(10);
                        });
                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("Fecha:").Bold().FontSize(11);
                            col.Item().Text(model.Receta?.Fecha.ToString("dd/MM/yyyy") ?? "").FontSize(10);
                        });
                    });

                    col.Item().PaddingTop(5).LineHorizontal(0.5f).LineColor("#004169");

                    col.Item().Text($"Paciente: {model.Paciente?.Nombre} {model.Paciente?.Apellido}")
                        .FontSize(11).FontColor(Colors.Grey.Medium);

                    col.Item().PaddingTop(5).LineHorizontal(0.5f).LineColor("#004169");

                    col.Item().PaddingTop(10).Column(detalleCol =>
                    {
                        foreach (var item in model.DetallesReceta ?? new())
                        {
                            double total = 0;
                            int tiempoInt = 0, diasInt = 0;
                            double cantidadDeci = 0;
                            bool tieneTotal = !string.IsNullOrEmpty(item.Cantidad)
                                && !string.IsNullOrEmpty(item.DosisTiempo)
                                && !string.IsNullOrEmpty(item.DosisDias)
                                && int.TryParse(item.DosisTiempo, out tiempoInt)
                                && double.TryParse(item.Cantidad, out cantidadDeci)
                                && int.TryParse(item.DosisDias, out diasInt);
                            if (tieneTotal)
                            {
                                total = cantidadDeci * (24.0 / tiempoInt) * diasInt;
                            }

                            detalleCol.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text(item.Medicamento?.ToUpper() ?? "").Bold().FontSize(11);
                                    if (total != 0)
                                        c.Item().Text($"Total: {total}").FontSize(9).FontColor(Colors.Grey.Medium);
                                });
                            });
                            detalleCol.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Black);
                            if (!string.IsNullOrEmpty(item.Instrucciones))
                            {
                                detalleCol.Item().Text($"Instrucciones: {item.Instrucciones}").FontSize(10).FontColor(Colors.Grey.Medium);
                                detalleCol.Item().PaddingBottom(5);
                            }
                        }

                        if (!string.IsNullOrEmpty(model.Receta?.Descripcion))
                        {
                            detalleCol.Item().PaddingTop(5).Text($"Otras observaciones: {model.Receta.Descripcion}")
                                .FontSize(10).FontColor(Colors.Grey.Medium);
                        }

                        detalleCol.Item().PaddingTop(5).Text("- OMITIR CUALQUIER PRESCRIPCION DESPUES DE ESTA LINEA -")
                            .Bold().FontSize(9).FontColor(Colors.Red.Medium);

                        if (model.CitaProx != default)
                        {
                            detalleCol.Item().PaddingTop(5).Text($"Próxima cita: {model.CitaProx:dd/MM/yyyy HH:mm:ss}")
                                .FontSize(10).FontColor(Colors.Grey.Medium);
                        }
                    });
                });

                page.Footer().Column(footerCol =>
                {
                    footerCol.Item().LineHorizontal(0.5f).LineColor("#004169");
                    footerCol.Item().AlignCenter().Text("- NO PERMITA QUE LE CAMBIEN LA RECETA -")
                        .Bold().FontSize(10).FontColor(Colors.Red.Medium);
                    footerCol.Item().AlignCenter().Text("Clínicas Dr. Manuel Hernández - Traumatología y Ortopedia del Deporte")
                        .Bold().FontSize(11);
                    footerCol.Item().AlignCenter().Text("Tel: 7832-6115 | Cel: 3787-1021 | dr.manuelhernandeztyo@gmail.com | drmanuelhernandez.com")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
footerCol.Item().PaddingTop(5).AlignCenter().Text("Impreso por Clínicas Dr. Manuel Hernández").FontSize(8).FontColor(Colors.Grey.Lighten1);
                });
            });
        });

        return document.GeneratePdf();
    }
}