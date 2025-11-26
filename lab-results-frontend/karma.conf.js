// Karma configuration file, see link for more information
// https://karma-runner.github.io/1.0/config/configuration-file.html

module.exports = function (config) {
    config.set({
        basePath: '',
        frameworks: ['jasmine', '@angular-devkit/build-angular'],
        plugins: [
            require('karma-jasmine'),
            require('karma-chrome-launcher'),
            require('karma-jasmine-html-reporter'),
            require('karma-coverage'),
            require('@angular-devkit/build-angular/plugins/karma')
        ],
        client: {
            jasmine: {
                // you can add configuration options for Jasmine here
                // the possible options are listed at https://jasmine.github.io/api/edge/Configuration.html
                // for example, you can disable the random execution order
                // random: false
            },
            clearContext: false // leave Jasmine Spec Runner output visible in browser
        },
        jasmineHtmlReporter: {
            suppressAll: true // removes the duplicated traces
        },
        coverageReporter: {
            dir: require('path').join(__dirname, './coverage/lab-results-frontend'),
            subdir: '.',
            reporters: [
                { type: 'html' },
                { type: 'text-summary' }
            ]
        },
        reporters: ['progress', 'kjhtml'],
        browsers: ['ThoriumHeadless'],
        customLaunchers: {
            ThoriumHeadless: {
                base: 'ChromeHeadless',
                flags: [
                    '--no-sandbox',
                    '--disable-web-security',
                    '--disable-features=VizDisplayCompositor',
                    '--disable-dev-shm-usage',
                    '--remote-debugging-port=9222'
                ]
            },
            Thorium: {
                base: 'Chrome',
                flags: [
                    '--no-sandbox',
                    '--disable-web-security',
                    '--disable-features=VizDisplayCompositor'
                ]
            }
        },
        restartOnFileChange: true,
        singleRun: false
    });

    // Try to detect Thorium installation
    const os = require('os');
    const path = require('path');

    if (os.platform() === 'linux') {
        // Common Thorium installation paths on Linux
        const thoriumPaths = [
            '/usr/bin/thorium-browser',
            '/usr/local/bin/thorium-browser',
            '/opt/thorium/thorium-browser',
            path.join(os.homedir(), '.local/bin/thorium-browser'),
            path.join(os.homedir(), 'Applications/thorium-browser')
        ];

        const fs = require('fs');
        for (const thoriumPath of thoriumPaths) {
            if (fs.existsSync(thoriumPath)) {
                process.env.CHROME_BIN = thoriumPath;
                break;
            }
        }
    }

    // If Thorium not found, try to use any Chromium-based browser
    if (!process.env.CHROME_BIN) {
        const chromiumPaths = [
            '/usr/bin/chromium-browser',
            '/usr/bin/chromium',
            '/usr/bin/google-chrome',
            '/usr/bin/google-chrome-stable'
        ];

        const fs = require('fs');
        for (const chromiumPath of chromiumPaths) {
            if (fs.existsSync(chromiumPath)) {
                process.env.CHROME_BIN = chromiumPath;
                break;
            }
        }
    }
};