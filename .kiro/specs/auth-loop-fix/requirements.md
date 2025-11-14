# Authentication Loop Fix Requirements

## Introduction

The application is experiencing an infinite authentication loop when users click "Laboratory Tests". When the `/api/tests/qualified` endpoint returns a 401 Unauthorized error, the auth interceptor calls `logout()`, which makes another HTTP request to `/api/auth/logout`. Since the token is invalid, this logout request also returns 401, triggering the interceptor again and creating an infinite loop of logout attempts.

## Glossary

- **Auth_Service**: The Angular service responsible for authentication state management
- **Auth_Interceptor**: The HTTP interceptor that handles authentication headers and 401 errors
- **Logout_Endpoint**: The backend API endpoint `/api/auth/logout` that requires authorization
- **Token_Validation**: The process of checking if a stored authentication token is still valid

## Requirements

### Requirement 1

**User Story:** As a user with an invalid or expired token, I want the application to handle authentication failures gracefully without creating infinite loops, so that I can be redirected to login without browser performance issues.

#### Acceptance Criteria

1. WHEN THE Auth_Interceptor receives a 401 error, THE Auth_Service SHALL clear the authentication state locally without making additional HTTP requests
2. WHEN THE Auth_Service clears authentication state, THE Auth_Service SHALL remove the token and user data from local storage immediately
3. WHEN THE Auth_Service clears authentication state, THE Auth_Service SHALL redirect the user to the login page only if not already on a public route
4. IF THE logout operation requires server-side logging, THEN THE Auth_Service SHALL only attempt the HTTP logout request when the token is known to be valid
5. WHEN THE Auth_Service performs local logout, THE Auth_Service SHALL update all reactive signals to reflect the unauthenticated state

### Requirement 2

**User Story:** As a developer, I want the logout functionality to work correctly in both user-initiated and automatic scenarios, so that the application maintains proper authentication state.

#### Acceptance Criteria

1. WHEN A user manually clicks logout, THE Auth_Service SHALL attempt to call the server logout endpoint for audit logging
2. WHEN THE server logout request fails, THE Auth_Service SHALL still clear local authentication state
3. WHEN THE Auth_Interceptor triggers automatic logout due to 401 errors, THE Auth_Service SHALL skip the server logout request
4. WHEN THE Auth_Service clears authentication state, THE Auth_Service SHALL ensure no pending HTTP requests continue to use the invalid token
5. WHEN THE logout process completes, THE Auth_Service SHALL emit the updated authentication state to all subscribers

### Requirement 3

**User Story:** As a user, I want to see appropriate error messages when authentication fails, so that I understand what happened and what to do next.

#### Acceptance Criteria

1. WHEN THE qualified tests API returns 401, THE Auth_Service SHALL log the authentication failure for debugging
2. WHEN THE Auth_Service redirects to login due to invalid token, THE Auth_Service SHALL not display error messages to the user
3. WHEN A user manually logs out, THE Auth_Service SHALL complete the logout process silently
4. WHEN THE Auth_Service encounters network errors during logout, THE Auth_Service SHALL not prevent local state clearing
5. WHEN THE authentication state changes, THE Auth_Service SHALL notify all components through reactive signals