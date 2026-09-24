# PDF export dependencies

Locally bundled browser distributions (MIT licenses included):

- jsPDF 4.2.1: https://www.npmjs.com/package/jspdf/v/4.2.1 (`dist/jspdf.umd.min.js`)
- jsPDF-AutoTable 5.0.8: https://www.npmjs.com/package/jspdf-autotable/v/5.0.8 (`dist/jspdf.plugin.autotable.mjs`)

Downloaded with `npm pack jspdf@4.2.1 jspdf-autotable@5.0.8`. Copy these distribution files and their licenses when updating. They load only when exporting a report; no CDN or external PDF service is used.
