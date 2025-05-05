/**
 * Excel parser for Buenaventura transaction upload
 * Requires SheetJS (xlsx) library to be loaded
 */

window.openBulkTransactionFileInput = () => {
    document.getElementById('fileInput').click();
}

// Parse Excel file from base64 string
window.parseExcelFile = function (base64Data) {
    try {
        // Check if xlsx library is loaded
        if (typeof XLSX === 'undefined') {
            console.error('XLSX library not loaded');
            return null;
        }

        // Convert base64 to array buffer
        const binaryString = window.atob(base64Data);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }
        const arrayBuffer = bytes.buffer;

        // Parse workbook
        const workbook = XLSX.read(arrayBuffer, { type: 'array' });
        
        // Get first sheet
        const firstSheetName = workbook.SheetNames[0];
        const worksheet = workbook.Sheets[firstSheetName];
        
        // Convert to JSON
        const jsonData = XLSX.utils.sheet_to_json(worksheet, { header: "A" });
        
        // Process the data to get column headers
        const headerRow = jsonData[6]; // Row 16 (0-indexed) contains headers but we've skipped blank rows
        const headers = {};
        
        // Map column letters to header names
        Object.keys(headerRow).forEach(key => {
            headers[key] = headerRow[key];
        });
        
        // Create array of objects with proper column names
        const result = [];
        for (let i = 7; i < jsonData.length; i++) {
            const row = jsonData[i];
            if (!row || Object.keys(row).length === 0) continue;
            
            const processedRow = {};
            Object.keys(row).forEach(key => {
                const headerName = headers[key] || key;
                processedRow[headerName] = row[key];
            });
            
            result.push(processedRow);
        }
        
        return JSON.stringify(result);
    } catch (error) {
        console.error('Error parsing Excel file:', error);
        return null;
    }
};
