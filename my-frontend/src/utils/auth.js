// Giải mã token
export function decodeToken(token) {
    try {
        return JSON.parse(atob(token.split(".")[1]));
    } catch (e) {
        return null;
    }
}

// Lấy token từ localStorage
export function getToken() {
    return localStorage.getItem("token");
}

// Lấy AccId từ token
export function getAccIdFromToken() {
    const token = getToken();
    if (!token) return null;

    const payload = decodeToken(token);
    if (!payload) return null;

    return payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
}

// Lấy role từ token
export function getRoleFromToken() {
    const token = getToken();
    if (!token) return null;

    const payload = decodeToken(token);
    if (!payload) return null;

    return payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
}

// Kiểm tra user đã đăng nhập chưa
export function isLoggedIn() {
    return !!getToken();
}

// Xóa token → logout
export function logout() {
    localStorage.removeItem("token");
    localStorage.removeItem("role");
    localStorage.removeItem("username");
}
