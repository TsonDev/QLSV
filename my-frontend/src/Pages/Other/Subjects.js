import React, { useState, useEffect } from "react";
import "./../App.css";

function Subjects() {
    const [subjects, setSubjects] = useState([]);
    const [sortAsc, setSortAsc] = useState(true);

    // 🔹 Các điều kiện lọc
    const [filterTerm, setFilterTerm] = useState("");
    const [filterType, setFilterType] = useState("");
    const [filterCredit, setFilterCredit] = useState("");

    useEffect(() => {
        fetch("https://localhost:7287/api/Subjects")
            .then((res) => res.json())
            .then((data) => setSubjects(data))
            .catch((err) => console.error("Lỗi khi kết nối API:", err));
    }, []);

    // 🔹 Nút sắp xếp theo kỳ
    const sortByTerm = () => {
        const sorted = [...subjects].sort((a, b) =>
            sortAsc
                ? a.curriculumTerm - b.curriculumTerm
                : b.curriculumTerm - a.curriculumTerm
        );
        setSubjects(sorted);
        setSortAsc(!sortAsc);
    };

    // 🔹 Hàm lọc nhiều điều kiện cùng lúc
    const filterSubjects = () => {
        let filtered = [...subjects];

        if (filterTerm) {
            filtered = filtered.filter(
                (s) => String(s.curriculumTerm) === String(filterTerm)
            );
        }

        if (filterType) {
            filtered = filtered.filter((s) =>
                s.type.toLowerCase().includes(filterType.toLowerCase())
            );
        }

        if (filterCredit) {
            filtered = filtered.filter(
                (s) => String(s.soTc) === String(filterCredit)
            );
        }

        return filtered;
    };

    // 🔹 Nút xóa toàn bộ bộ lọc
    const resetFilters = () => {
        setFilterTerm("");
        setFilterType("");
        setFilterCredit("");
    };

    return (
        <div>
            <h2 style={{ textAlign: "center" }}>Danh sách môn học</h2>


            {/* 🔸 Khu vực bộ lọc */}
            <div
                style={{
                    display: "flex",
                    justifyContent: "center",
                    gap: "15px",
                    alignItems: "center",
                    marginBottom: "20px",
                }}
            >
                {/* Lọc theo kỳ */}
                <label>
                    Kỳ:
                    <select
                        value={filterTerm}
                        onChange={(e) => setFilterTerm(e.target.value)}
                        style={{ marginLeft: "5px" }}
                    >
                        <option value="">Tất cả</option>
                        {[...Array(8)].map((_, i) => (
                            <option key={i + 1} value={i + 1}>
                                Kỳ {i + 1}
                            </option>
                        ))}
                    </select>
                </label>

                {/* Lọc theo Type */}
                <label>
                    Loại chứa:
                    <input
                        type="text"
                        placeholder="VD: LT"
                        value={filterType}
                        onChange={(e) => setFilterType(e.target.value)}
                        style={{
                            marginLeft: "5px",
                            padding: "5px 8px",
                            borderRadius: "6px",
                            border: "1px solid #ccc",
                        }}
                    />
                </label>

                {/* Lọc theo số tín chỉ */}
                <label>
                    Tín chỉ:
                    <select
                        value={filterCredit}
                        onChange={(e) => setFilterCredit(e.target.value)}
                        style={{ marginLeft: "5px" }}
                    >
                        <option value="">Tất cả</option>
                        {[2, 3, 4, 5].map((num) => (
                            <option key={num} value={num}>
                                {num}
                            </option>
                        ))}
                    </select>
                </label>

                {/* Nút reset */}
                <button
                    onClick={resetFilters}
                    style={{
                        backgroundColor: "#6c757d",
                        color: "white",
                        border: "none",
                        padding: "8px 14px",
                        borderRadius: "6px",
                        cursor: "pointer",
                    }}
                >
                    Xóa lọc
                </button>

                {/* Nút sắp xếp */}
                <button
                    onClick={sortByTerm}
                    style={{
                        backgroundColor: "#007bff",
                        color: "white",
                        border: "none",
                        padding: "8px 14px",
                        borderRadius: "6px",
                        cursor: "pointer",
                    }}
                >
                    Sắp xếp theo kỳ {sortAsc ? "▲" : "▼"}
                </button>
            </div>

            {/* 🔸 Bảng dữ liệu */}
            {subjects.length === 0 ? (
                <p>Đang tải...</p>
            ) : (
                <div className="table-container">
                    <table>
                        <thead>
                            <tr>
                                <th>Mã môn</th>
                                <th>Tên môn học</th>
                                <th>Loại</th>
                                <th>Tín chỉ</th>
                                <th>Kỳ</th>
                            </tr>
                        </thead>
                        <tbody>
                            {filterSubjects().map((s) => (
                                <tr key={s.id}>
                                    <td>{s.id}</td>
                                    <td>{s.name}</td>
                                    <td>{s.type}</td>
                                    <td>{s.soTc}</td>
                                    <td>{s.curriculumTerm}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
}

export default Subjects;
