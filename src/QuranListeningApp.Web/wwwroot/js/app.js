// File download function
window.downloadFile = (fileName, mimeType, base64String) => {
    const data = atob(base64String);
    const bytes = new Uint8Array(data.length);
    for (let i = 0; i < data.length; i++) {
        bytes[i] = data.charCodeAt(i);
    }
    
    const blob = new Blob([bytes], { type: mimeType });
    const url = window.URL.createObjectURL(blob);
    
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    link.style.display = 'none';
    
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    window.URL.revokeObjectURL(url);
};

// Toast notifications
window.showToast = (message, type = 'info') => {
    // Simple alert for now, can be enhanced with Bootstrap toasts later
    alert(message);
};

// Confirmation dialog
window.confirmDialog = (message) => {
    return confirm(message);
};