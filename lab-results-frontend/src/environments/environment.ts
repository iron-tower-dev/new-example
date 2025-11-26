export const environment = {
    production: false,
    apiUrl: 'http://localhost:5248/api',
    apiTimeout: 30000,
    enableLogging: true,
    enableDebugMode: true,
    cacheTimeout: 300000, // 5 minutes
    fileUpload: {
        maxFileSize: 50 * 1024 * 1024, // 50MB
        allowedExtensions: ['.dat', '.txt', '.csv', '.pdf', '.jpg', '.jpeg', '.png', '.bmp', '.tiff'],
        chunkSize: 1024 * 1024 // 1MB chunks
    },
    features: {
        enableServiceWorker: false,
        enableOfflineMode: false,
        enablePushNotifications: false
    }
};