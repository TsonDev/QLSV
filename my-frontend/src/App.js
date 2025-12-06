import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import DashboardLayout from "./Layout/DashboardLayout";
import Login from "./Pages/Login";

// Admin pages
import HomeAdmin from "./Pages/admin/Home";
import AccountsPage from "./Pages/admin/AccountsPage";
import ApprovePage from "./Pages/admin/ApprovePage";

import ProtectedRoute from "./Routes/ProtectedRoute";
// Teacher pages
import ClassPage from "./Pages/Teacher/ClassPage";
import PointInputPage from "./Pages/Teacher/TeacherScoresPage";

export default function App() {
   return (
      <BrowserRouter>
         <Routes>

            {/* Redirect "/" theo role */}
            <Route 
               path="/" 
               element={
                  localStorage.getItem("role") === "Admin" ? (
                     <Navigate to="/admin/home" replace />
                  ) : localStorage.getItem("role") === "Teacher" ? (
                     <Navigate to="/teacher/home" replace />
                  ) : localStorage.getItem("role") === "Advisor" ? (
                     <Navigate to="/advisor/home" replace />
                  ) : (
                     <Navigate to="/login" replace />
                  )
               }
            />

            {/* Public */}
            <Route path="/login" element={<Login />} />
            <Route path="/not-authorized" element={<div>Không có quyền truy cập</div>} />
            

            {/* ADMIN */}
            <Route
               path="/admin"
               element={
                  <ProtectedRoute allowedRoles={["Admin"]}>
                     <DashboardLayout />
                  </ProtectedRoute>
               }
            >
               <Route path="home" element={<HomeAdmin />} />
               <Route path="accounts" element={<AccountsPage />} />
               <Route path="teachers" element={<div>Quản lý giảng viên</div>} />
               <Route path="advisor" element={<div>Quản lý cố vấn</div>} />
               <Route path="classes" element={<ApprovePage />} />
               <Route path="statistics" element={<div>Thống kê</div>} />
            </Route>

            {/* TEACHER */}
            <Route
               path="/teacher"
               element={
                  <ProtectedRoute allowedRoles={["Teacher"]}>
                     <DashboardLayout />
                  </ProtectedRoute>
               }
            >
               <Route path="home" element={<div>Trang giáo viên</div>} />
               <Route path="classes" element={<ClassPage />} />
               <Route path="scores" element={<PointInputPage />} />
               {/* <Route path="score/:classId" element={<PointInputPage />} /> */}

            </Route>

            {/* ADVISOR */}
            <Route
               path="/advisor"
               element={
                  <ProtectedRoute allowedRoles={["Advisor"]}>
                     <DashboardLayout />
                  </ProtectedRoute>
               }
            >
               <Route path="home" element={<div>Trang cố vấn học tập</div>} />
               <Route path="students" element={<div>Sinh viên cố vấn</div>} />
            </Route>

         </Routes>
      </BrowserRouter>
   );
}
