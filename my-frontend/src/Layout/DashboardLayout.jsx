import Sidebar from "../Components/Sidebar";
import { Outlet, useNavigate } from "react-router-dom";
import "./Layout.css";
import { useEffect, useState } from "react";

// 👉 Import auth utils
import { getToken, getAccIdFromToken, logout } from "../utils/auth";

export default function DashboardLayout() {
    const [user, setUser] = useState(null);
    const [openMenu, setOpenMenu] = useState(false);
    const navigate = useNavigate();

    const handleLogout = () => {
        logout();
        navigate("/login");
    };

    useEffect(() => {
        const token = getToken();

        if (!token) {
            navigate("/login");
            return;
        }

        //  Lấy accId bằng auth utils (không decode thủ công nữa)
        const accId = getAccIdFromToken();
        if (!accId) {
            logout();
            navigate("/login");
            return;
        }

        //  Gọi API lấy user info
        fetch(`https://localhost:7287/api/Accounts/${accId}`, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json"
            }
        })
            .then(res => res.json())
            .then(data => {
                setUser(data);
                localStorage.setItem("username", data.username);
            })
            .catch(() => {
                logout();
                navigate("/login");
            });

    }, [navigate]);

    return (
        <div className="layout-wrapper">

            <Sidebar />

            <div className="right-side">

                <div className="topbar">

                    <div
                        className="username-box"
                        onClick={() => setOpenMenu(!openMenu)}
                        style={{ cursor: "pointer" }}
                    >
                        👤 {user ? user.username : "Loading..."}
                    </div>

                    {openMenu && (
                        <div className="dropdown-menu">
                            <button onClick={handleLogout}>Đăng xuất</button>
                        </div>
                    )}
                </div>

                <div className="content">
                    <Outlet />
                </div>

            </div>
        </div>
    );
}
