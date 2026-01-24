// Theme management functions
window.themeManager = {
    applyTheme: function(theme) {
        const body = document.body;
        
        // Remove existing theme classes
        body.classList.remove('dark-mode');
        
        // Apply new theme
        if (theme === 'Dark') {
            body.classList.add('dark-mode');
        }
        
        // Store theme preference in localStorage
        localStorage.setItem('theme', theme);
        
        // Refresh the page to apply theme changes
        setTimeout(() => {
            window.location.reload();
        }, 100);
    },
    
    loadSavedTheme: function() {
        const savedTheme = localStorage.getItem('theme');
        if (savedTheme) {
            const body = document.body;
            body.classList.remove('dark-mode');
            
            if (savedTheme === 'Dark') {
                body.classList.add('dark-mode');
            }
        }
    }
};
