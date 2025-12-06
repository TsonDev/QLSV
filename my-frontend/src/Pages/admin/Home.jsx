import { useEffect, useState } from "react";

export default function Home() {

    const [stats, setStats] = useState({
        students: 0,
        teachers: 0,
        advisors: 0,
        classes: 0,
        accounts: 0
    });

    useEffect(() => {
        const token = localStorage.getItem("token");

        Promise.all([
            fetch("https://localhost:7287/api/Students/list-basic", { headers: { Authorization: `Bearer ${token}` } }),
            fetch("https://localhost:7287/api/Teachers", { headers: { Authorization: `Bearer ${token}` } }),
            fetch("https://localhost:7287/api/Advisors", { headers: { Authorization: `Bearer ${token}` } }),
            fetch("https://localhost:7287/api/Classes", { headers: { Authorization: `Bearer ${token}` } }),
            fetch("https://localhost:7287/api/Accounts", { headers: { Authorization: `Bearer ${token}` } })
        ])
            .then(async ([stu, tea, adv, cla, acc]) => ({
                students: await stu.json(),
                teachers: await tea.json(),
                advisors: await adv.json(),
                classes: await cla.json(),
                accounts: await acc.json()
            }))
            .then(data => {
                setStats({
                    students: data.students.length,
                    teachers: data.teachers.length,
                    advisors: data.advisors.length,
                    classes: data.classes.length,
                    accounts: data.accounts.length
                });
            })
            .catch(err => console.error("Lỗi lấy thống kê:", err));

    }, []);

    return (
        <div className="home-wrapper">
            
            {/* Giữ nguyên toàn bộ CSS của bạn */}
            <style>{`
                .home-wrapper {
                    padding: 20px;
                    font-family: Arial, sans-serif;
                }

                .welcome-box {
                    background: white;
                    padding: 25px;
                    border-radius: 10px;
                    box-shadow: 0 2px 5px rgba(0,0,0,0.1);
                    margin-bottom: 25px;
                }

                .welcome-title {
                    margin: 0;
                    font-size: 26px;
                    font-weight: bold;
                }

                .welcome-sub {
                    margin-top: 8px;
                    color: #666;
                }

                .stats-container {
                    display: grid;
                    grid-template-columns: repeat(4, 1fr);
                    gap: 20px;
                    margin-bottom: 30px;
                }

                .stat-card {
                    background: white;
                    padding: 25px;
                    border-radius: 10px;
                    text-align: center;
                    box-shadow: 0 3px 8px rgba(0,0,0,0.1);
                    cursor: pointer;
                    transition: 0.2s;
                }

                .stat-card:hover {
                    transform: translateY(-4px);
                }

                .stat-value {
                    font-size: 32px;
                    font-weight: bold;
                    color: #4c4cff;
                    margin-bottom: 8px;
                }

                .stat-label {
                    font-size: 16px;
                    color: #555;
                }

                .quick-actions-title {
                    font-size: 20px;
                    margin-bottom: 10px;
                }

                .quick-actions-grid {
                    display: grid;
                    grid-template-columns: repeat(3, 1fr);
                    gap: 20px;
                }

                .action-card {
                    background: #ffffff;
                    padding: 20px;
                    border-radius: 10px;
                    text-align: left;
                    box-shadow: 0 2px 5px rgba(0,0,0,0.1);
                    cursor: pointer;
                    transition: 0.2s;
                }

                .action-card:hover {
                    background: #f5f6ff;
                    transform: translateY(-3px);
                }

                .action-title {
                    margin: 0;
                    font-size: 18px;
                    font-weight: bold;
                    color: #333;
                }

                .action-desc {
                    margin-top: 6px;
                    color: #777;
                    font-size: 14px;
                }
            `}</style>

            {/* Banner chào mừng */}
            <div className="welcome-box">
                <h1 className="welcome-title">Chào mừng đến hệ thống Quản lý Sinh viên</h1>
                <p className="welcome-sub">Quản trị thông tin sinh viên, lớp học phần, điểm số và thống kê.</p>
            </div>

            {/* 4 ô thống kê (ĐÃ CÓ DỮ LIỆU THẬT) */}
            <div className="stats-container">
                <div className="stat-card">
                    <div className="stat-value">{stats.students}</div>
                    <div className="stat-label">Sinh viên</div>
                </div>

                <div className="stat-card">
                    <div className="stat-value">{stats.teachers}</div>
                    <div className="stat-label">Giảng viên</div>
                </div>

                <div className="stat-card">
                    <div className="stat-value">{stats.advisors}</div>
                    <div className="stat-label">Cố vấn học tập</div>
                </div>

                <div className="stat-card">
                    <div className="stat-value">{stats.classes}</div>
                    <div className="stat-label">Lớp học phần</div>
                </div>

                <div className="stat-card">
                    <div className="stat-value">{stats.accounts}</div>
                    <div className="stat-label">Tài khoản</div>
                </div>
            </div>

                {/* Biểu đồ thống kê tổng quát */}
<h3 style={{ marginTop: "40px", marginBottom: "15px" }}>Thống kê tổng quát</h3>

<style>{`
    .chart-container {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 30px;
        margin-top: 15px;
    }

    .chart-box {
        background: white;
        padding: 20px;
        border-radius: 10px;
        box-shadow: 0 2px 6px rgba(0,0,0,0.1);
    }

    /* === BAR CHART CĂN ĐÚNG === */
    .bar-wrapper {
        height: 180px;
        display: flex;
        align-items: flex-end;
        justify-content: space-evenly;
        margin-top: 10px;
    }

    .bar {
        width: 55px;
        border-radius: 6px 6px 0 0;
        transition: 0.2s;
    }

    .bar:hover {
        transform: translateY(-5px);
        opacity: 0.9;
    }

    .bar-labels {
        display: flex;
        justify-content: space-evenly;
        margin-top: 10px;
        color: #444;
    }

    /* === LINE CHART CHUẨN === */
    .line-chart-wrapper {
        position: relative;
        height: 220px;
        margin-top: 20px;
    }

    .line-point {
        width: 12px;
        height: 12px;
        background: #ff5c5c;
        border-radius: 50%;
        position: absolute;
        transform: translate(-50%, -50%);
    }

    .line-path {
        position: absolute;
        height: 2px;
        background: #ff5c5c;
        transform-origin: left;
    }

    .line-labels {
        display: flex;
        justify-content: space-between;
        margin-top: 15px;
        color: #444;
    }
`}</style>

<div className="chart-container">

    {/* BIỂU ĐỒ CỘT */}
    <div className="chart-box">
        <h4 style={{ margin: 0 }}>Tỷ lệ đậu – rớt</h4>

        <div className="bar-wrapper">
            <div className="bar" style={{ height: "150px", background: "#4c4cff" }}></div>
            <div className="bar" style={{ height: "90px", background: "#ff5c5c" }}></div>
        </div>

        <div className="bar-labels">
            <span>Đậu</span>
            <span>Rớt</span>
        </div>
    </div>

    {/* BIỂU ĐỒ ĐƯỜNG */}
    <div className="chart-box">
        <h4 style={{ margin: 0 }}>GPA trung bình theo học kỳ</h4>

        <div className="line-chart-wrapper">

            {/* Điểm dữ liệu (chỉnh lại 100% căn giữa container) */}
            <div className="line-point" style={{ left: "10%", top: "65%" }}></div>
            <div className="line-point" style={{ left: "30%", top: "50%" }}></div>
            <div className="line-point" style={{ left: "50%", top: "45%" }}></div>
            <div className="line-point" style={{ left: "70%", top: "58%" }}></div>
            <div className="line-point" style={{ left: "90%", top: "40%" }}></div>

            {/* Các đoạn nối chuẩn */}
            <div className="line-path" style={{
                left: "10%", top: "65%",
                width: "20%", transform: "rotate(-32deg)"
            }}></div>

            <div className="line-path" style={{
                left: "30%", top: "50%",
                width: "20%", transform: "rotate(-13deg)"
            }}></div>

            <div className="line-path" style={{
                left: "50%", top: "45%",
                width: "20%", transform: "rotate(28deg)"
            }}></div>

            <div className="line-path" style={{
                left: "70%", top: "58%",
                width: "20%", transform: "rotate(-38deg)"
            }}></div>
        </div>

        {/* Nhãn kỳ */}
        <div className="line-labels">
            <span>Kỳ 1</span>
            <span>Kỳ 2</span>
            <span>Kỳ 3</span>
            <span>Kỳ 4</span>
            <span>Kỳ 5</span>
        </div>
    </div>

</div>

            <h3 className="quick-actions-title">Chức năng nhanh</h3>
            <div className="quick-actions-grid">
                <div className="action-card">
                    <h4 className="action-title">Quản lý Sinh viên</h4>
                    <p className="action-desc">Xem danh sách, thêm mới, chỉnh sửa thông tin.</p>
                </div>

                <div className="action-card">
                    <h4 className="action-title">Quản lý Giảng viên</h4>
                    <p className="action-desc">Cập nhật thông tin, xem lịch giảng dạy.</p>
                </div>

                <div className="action-card">
                    <h4 className="action-title">Quản lý Lớp học phần</h4>
                    <p className="action-desc">Tạo lớp, gán giảng viên, kiểm tra trùng lịch.</p>
                </div>

                <div className="action-card">
                    <h4 className="action-title">Nhập điểm</h4>
                    <p className="action-desc">Giảng viên nhập điểm quá trình, giữa kỳ, cuối kỳ.</p>
                </div>

                <div className="action-card">
                    <h4 className="action-title">Điểm - Thống kê</h4>
                    <p className="action-desc">Xem biểu đồ, tỷ lệ đậu – rớt, phân tích học tập.</p>
                </div>

                <div className="action-card">
                    <h4 className="action-title">Quản lý tài khoản</h4>
                    <p className="action-desc">Tạo tài khoản, khóa/mở, phân quyền.</p>
                </div>
            </div>

        </div>
    );
}
