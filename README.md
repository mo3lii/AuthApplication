# Authentication & Authorization Web API  

This Web API project implements token-based authentication using JWT (JSON Web Tokens) and role-based authorization to provide secure access to resources. It includes structured error handling, token revocation, and supports refreshing expired tokens.  

## Features  
- Stateless authentication using short-lived access tokens (e.g., 20 minutes) and refresh tokens for renewing access.  
- Role-based authorization to control access, with roles such as `SuperAdmin`, `Admin`, and `User`.  
- Structured error handling with custom middleware for debugging and auditing.  
- Logging of failed login attempts for security purposes.  

## User Flow  
1. **Registration**: Users register with a username, email, and password, and are assigned a role (`User` by default).  
2. **Login**: Users authenticate with their username and password to receive an access token and refresh token.  
3. **Access Secured Resources**: JWT is included in the `Authorization` header for API requests, and the server validates the token before processing.  
4. **Token Refresh**: The client uses the refresh token to obtain a new access token when the current one expires.  
5. **Logout**: Refresh tokens are revoked to prevent further use.  

## API Endpoints  
### **UserController**  
1. `POST /api/user/register`: Registers a new user.  
   - **Request Body**:  
     ```json
     {
       "userName": "string",
       "email": "string",
       "password": "string"
     }
     ```  
   - **Responses**:  
     - **Success (201)**: User successfully registered.  
     - **Error (400)**: Validation errors (e.g., weak password, duplicate email).  

2. `POST /api/user/login`: Authenticates a user and issues tokens.  
   - **Request Body**:  
     ```json
     {
       "userName": "string",
       "password": "string"
     }
     ```  
   - **Responses**:  
     - **Success (200)**:  
       ```json
       {
         "token": "string",
         "refreshToken": "string",
         "roles": ["string"]
       }
       ```  
     - **Error (401)**: Invalid credentials.  

3. `PUT /api/user/logout`: Logs out a user by revoking their refresh token.  
   - **Responses**:  
     - **Success (200)**: User successfully logged out.  
     - **Error (401)**: User not authenticated.  

4. `POST /api/user/refresh`: Refreshes the access token.  
   - **Request Body**:  
     ```json
     {
       "refreshToken": "string"
     }
     ```  
   - **Responses**:  
     - **Success (200)**:  
       ```json
       {
         "token": "string",
         "refreshToken": "string"
       }
       ```  
     - **Error (401)**: Invalid or expired refresh token.  

### **TestController**  
- Includes multiple endpoints to test role-based authentication and authorization.  

## Error Handling  
- Global exception handling using custom middleware (`ExceptionMiddleware`) formats unhandled exceptions into structured JSON responses.  
- **Error Response Format**:  
  ```json
  {
    "success": false,
    "message": "string",
    "details": "string"
  }
