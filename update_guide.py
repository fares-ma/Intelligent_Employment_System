import re

with open('Frontend_Integration_Guide.md', 'r', encoding='utf-8') as f:
    content = f.read()

new_section = """
## 🗺 دليل صفحات واجهة المستخدم (Frontend Pages & UI Guide)

بناءً على تحليل الـ APIs الخاصة بالمصادقة (Auth)، المرشحين (Candidates)، والشركات (Company)، إليك الصفحات التي يجب على مطور الواجهة الأمامية إنشاؤها ومحتوياتها:

### 1. قسم المصادقة (Authentication Pages)
#### أ. صفحة تسجيل دخول المرشح والشركة (Login Page)
- **المسار (Route):** `/login`
- **المكونات:** 
  - حقل البريد الإلكتروني (Email)
  - حقل كلمة المرور (Password)
  - زر "تسجيل الدخول" (ينادي `POST /api/Auth/login`)
  - رابط "نسيت كلمة المرور" (يوجه إلى `/forgot-password`)
  - روابط للتسجيل: "إنشاء حساب كمرشح" / "تسجيل شركة جديدة"

#### ب. صفحة تسجيل المرشح (Candidate Registration)
- **المسار (Route):** `/register/candidate`
- **المكونات:**
  - حقول: الاسم الأول، الاسم الأخير، البريد الإلكتروني، كلمة المرور، تأكيد كلمة المرور.
  - زر "تسجيل" (ينادي `POST /api/Auth/register`)

#### ج. صفحة تسجيل شركة جديدة - مسؤول (Company Registration)
- **المسار (Route):** `/register/company`
- **المكونات:**
  - بيانات مسؤول التوظيف: الاسم الأول، الاسم الأخير، البريد، كلمة المرور.
  - بيانات الشركة: اسم الشركة، الرقم الضريبي (TaxNumber)، الموقع الإلكتروني (اختياري).
  - زر "تسجيل الشركة" (ينادي `POST /api/Auth/register/company`)

#### د. صفحة تسجيل موظف توظيف - عبر دعوة (Recruiter Registration via Invite)
- **المسار (Route):** `/register/recruiter`
- **المكونات:**
  - حقول: الاسم، البريد، كلمة المرور، **كود الدعوة (Invite Code)**.
  - زر "تسجيل" (ينادي `POST /api/Auth/register/recruiter`)

---

### 2. قسم المرشح (Candidate Portal)
#### أ. صفحة الملف الشخصي للمرشح (Candidate Profile)
- **المسار (Route):** `/candidate/profile`
- **المكونات:**
  - **عرض البيانات:** جلب البيانات عبر `GET /api/Candidates/profile`.
  - **تعديل البيانات الأساسية:** فورم لتعديل العنوان، المسمى الوظيفي، النبذة، روابط التواصل (ينادي `PUT /api/Candidates/profile`).
  - **الصورة الشخصية:** مكون لرفع وتعديل الصورة (ينادي `PUT /api/Candidates/profile-picture` - FormData).
  - **السير الذاتية (Resumes):** قائمة بالسير الذاتية المرفوعة.
    - زر لرفع سيرة ذاتية جديدة (ينادي `POST /api/Candidates/resume` - FormData PDF/DOCX).
    - زر لحذف السيرة (ينادي `DELETE /api/Candidates/resume/{id}`).
    - زر "توليد CV بالذكاء الاصطناعي" (ينادي `POST /api/Candidates/resume/{id}/generate-cv`).
  - **المهارات:** مكون لإضافة وحذف المهارات (ينادي `PUT /api/Candidates/skills`).

#### ب. صفحة الوظائف المحفوظة وتطبيقات العمل (Saved Jobs & Applications)
- **المسار (Route):** `/candidate/dashboard`
- **المكونات:**
  - **الوظائف المحفوظة:** جدول يعرض الوظائف (ينادي `GET /api/Candidates/saved-jobs`).
    - زر إزالة من المحفوظات (ينادي `POST /api/Candidates/saved-jobs/{id}`).
  - **سجل التقديمات:** جدول يعرض حالة الطلبات (Pending, Accepted...) (ينادي `GET /api/Candidates/applications`).

---

### 3. قسم الشركة ومسؤول التوظيف (Company & Recruiter Portal)
#### أ. صفحة إعدادات الشركة (Company Settings - Admin Only)
- **المسار (Route):** `/company/settings`
- **المكونات:**
  - **عرض تفاصيل الشركة:** (ينادي `GET /api/Company/{companyId}`).
  - **تعديل بيانات الشركة:** فورم لتعديل الاسم، اللوجو، الوصف، الموقع (ينادي `PUT /api/Company/{companyId}`).
  - **نقل الصلاحية (Transfer Admin):** فورم لاختيار موظف توظيف آخر لنقل صلاحية الإدارة إليه (ينادي `POST /api/Company/{companyId}/transfer-admin`).
  - **إحصائيات الدعوات:** عرض عدد الدعوات النشطة (ينادي `GET /api/Company/{companyId}/active-invitations-count`).

"""

if "## 🗺 دليل صفحات واجهة المستخدم" not in content:
    # Insert right after Project Overview
    parts = content.split("## 🛠 2. Development Setup")
    if len(parts) == 2:
        new_content = parts[0] + new_section + "\n\n## 🛠 2. Development Setup" + parts[1]
        with open('Frontend_Integration_Guide.md', 'w', encoding='utf-8') as f:
            f.write(new_content)
        print("Updated successfully")
    else:
        print("Could not find insertion point")
else:
    print("Section already exists")
