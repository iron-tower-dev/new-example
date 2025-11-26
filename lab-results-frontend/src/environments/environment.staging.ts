export const environment = {
    production: false,
    apiUrl: 'http://staging-server:8081/api',
    apiTimeout: 45000,
    enableLogging: true,
    enableDebugMode: true,
    cacheTimeout: 600000, // 10 minutes
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