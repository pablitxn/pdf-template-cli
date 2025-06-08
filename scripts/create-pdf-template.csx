#!/usr/bin/env dotnet-script
#r "nuget: Aspose.Words, 24.12.0"

using Aspose.Words;
using Aspose.Words.Saving;

var doc = new Document();
var builder = new DocumentBuilder(doc);

// Title
builder.Font.Size = 18;
builder.Font.Bold = true;
builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
builder.Writeln("CONTRATO DE SERVICIOS");
builder.Writeln();

// Body
builder.Font.Size = 12;
builder.Font.Bold = false;
builder.ParagraphFormat.Alignment = ParagraphAlignment.Justify;

builder.Writeln("En {{ciudad}}, a {{fecha}}, entre {{nombre_cliente}} con RUT {{rut_cliente}} (en adelante, el \"Cliente\") y {{nombre_proveedor}} con RUT {{rut_proveedor}} (en adelante, el \"Proveedor\"), se celebra el siguiente contrato:");
builder.Writeln();

builder.Font.Bold = true;
builder.Writeln("PRIMERO: OBJETO DEL CONTRATO");
builder.Font.Bold = false;
builder.Writeln("El Proveedor se compromete a prestar los siguientes servicios: {{descripcion_servicios}}");
builder.Writeln();

builder.Font.Bold = true;
builder.Writeln("SEGUNDO: DURACIÓN");
builder.Font.Bold = false;
builder.Writeln("El presente contrato tendrá una duración de {{duracion}}, comenzando el {{fecha_inicio}} y finalizando el {{fecha_termino}}.");
builder.Writeln();

builder.Font.Bold = true;
builder.Writeln("TERCERO: PRECIO Y FORMA DE PAGO");
builder.Font.Bold = false;
builder.Writeln("El Cliente pagará al Proveedor la suma de {{monto_total}} ({{monto_palabras}}) por los servicios prestados.");
builder.Writeln("Forma de pago: {{forma_pago}}");
builder.Writeln();

builder.Font.Bold = true;
builder.Writeln("CUARTO: CONFIDENCIALIDAD");
builder.Font.Bold = false;
builder.Writeln("Ambas partes se comprometen a mantener confidencialidad respecto a {{informacion_confidencial}}");
builder.Writeln();

builder.Writeln("Para constancia, firman:");
builder.Writeln();
builder.Writeln();

// Signature lines
builder.Writeln("_______________________               _______________________");
builder.Writeln("{{nombre_cliente}}                    {{nombre_proveedor}}");
builder.Writeln("RUT: {{rut_cliente}}                  RUT: {{rut_proveedor}}");

// Save as PDF
var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "samples/templates/contract-template.pdf");
doc.Save(outputPath, SaveFormat.Pdf);

Console.WriteLine($"PDF template created at: {outputPath}");