import "./vendor/jspdf/jspdf.umd.min.js";
import { autoTable } from "./vendor/jspdf-autotable/jspdf.plugin.autotable.mjs";

export function createDocument(report) {
    const doc = new globalThis.jspdf.jsPDF({ orientation: "landscape", unit: "mm", format: "a4" });
    const title = "Expense Categories by Month";
    const period = `${report.months[0]} - ${report.months.at(-1)}`;
    doc.setProperties({ title: `${title} (${period})` });
    const filterLines = report.filters ? doc.setFontSize(9).splitTextToSize(report.filters, 273) : [];
    const tableTop = 29 + filterLines.length * 4;

    // Keep columns readable even for projects spanning many years.
    for (let offset = 0; offset < report.months.length; offset += 6) {
        if (offset > 0) doc.addPage();
        const months = report.months.slice(offset, offset + 6);
        autoTable(doc, {
            head: [["Category", ...months, "Period total"]],
            body: report.rows.map(row => [row.name, ...row.amounts.slice(offset, offset + 6), row.total]),
            foot: [["Total", ...report.monthlyTotals.slice(offset, offset + 6), report.total]],
            startY: tableTop,
            margin: { top: tableTop, bottom: 15, left: 12, right: 12 },
            theme: "grid",
            styles: { fontSize: 9, cellPadding: 2.5, halign: "right", overflow: "linebreak" },
            columnStyles: { 0: { cellWidth: 53, halign: "left" } },
            headStyles: { fillColor: [38, 70, 83] },
            footStyles: { fillColor: [230, 237, 240], textColor: [20, 30, 35] },
            showFoot: "lastPage",
            rowPageBreak: "avoid",
            willDrawPage: () => {
                doc.setFont("helvetica", "bold").setFontSize(16).setTextColor(20, 30, 35);
                doc.text(title, 12, 13);
                doc.setFont("helvetica", "normal").setFontSize(10);
                doc.text(`Period: ${period} | Months: ${months[0]} - ${months.at(-1)}`, 12, 20);
                if (filterLines.length) doc.setFontSize(9).text(filterLines, 12, 26);
            }
        });
    }

    const pages = doc.getNumberOfPages();
    for (let page = 1; page <= pages; page++) {
        doc.setPage(page);
        doc.setFont("helvetica", "normal").setFontSize(8).setTextColor(80);
        doc.text(`Page ${page} of ${pages}`, 285, 202, { align: "right" });
    }
    return doc;
}

export function download(report) {
    createDocument(report).save(report.filename);
}
