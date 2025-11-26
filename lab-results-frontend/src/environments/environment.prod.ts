export const environment = {
    production: true,
    apiUrl: '/api',
    apiTimeout: 60000,
    enableLogging: false,
    enableDebugMode: false,
    cacheTimeout: 900000, // 15 minutes
    fileUpload: {
        maxFileSize: 50 * 1024 * 1024, // 50MB
        allowedExtensions: ['.dat', '.txt', '.csv', '.pdf', '.jpg', '.jpeg', '.png', '.bmp', '.tiff'],
        chunkSize: 1024 * 1024 // 1MB chunks
    },
    features: {
        enableServiceWorker: true,
        enableOfflineMode: true,
        enablePushNotifications: false
    }
};