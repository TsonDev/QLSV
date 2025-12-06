import { NavLink } from "react-router-dom";
import "./Sidebar.css";
import { getRoleFromToken } from "../utils/auth";

export default function Sidebar() {
    const role = getRoleFromToken();

    // MENU CHO TỪNG ROLE
    const menuByRole = {
        Admin: [
            { path: "/admin/home", label: "Trang chủ" },
            { path: "/admin/accounts", label: "Quản lý tài khoản" },
            { path: "/admin/teachers", label: "Quản lý giảng viên" },
            { path: "/admin/advisor", label: "Quản lý cố vấn" },
            { path: "/admin/classes", label: "Quản lý lớp" },
            { path: "/admin/statistics", label: "Thống kê" }
        ],

        Teacher: [
            { path: "/teacher/home", label: "Trang giáo viên" },
            { path: "/teacher/classes", label: "Lớp phụ trách" },
            { path: "/teacher/scores", label: "Nhập điểm" }
        ],

        Advisor: [
            { path: "/advisor/home", label: "Trang cố vấn" },
            { path: "/advisor/students", label: "Sinh viên tư vấn" }
        ]
    };

    const menuList = menuByRole[role] || [];

    return (
        <div className="sidebar">
            <h2 className="logo">QLSV</h2>

            <nav>
                {menuList.map((item, index) => (
                    <NavLink key={index} to={item.path} className="menu-item">
                        {item.label}
                    </NavLink>
                ))}
            </nav>
        </div>
    );
}
