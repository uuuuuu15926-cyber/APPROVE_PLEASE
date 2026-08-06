using UnityEngine;
using System.Collections.Generic;

// ─── DATA STRUCTURES ───────────────────────────────────────────

[System.Serializable]
public class HRProfileData
{
    public string fullName;
    public string department;
    public string employeeID;
    public int securityLevel; // 1-5
    public string workstationIP;
    public string photoSilhouetteID; // for picking avatar sprite
}

[System.Serializable]
public class ActionProposalData
{
    public string date;
    public string requestTitle;
    public string requestedAmount; // e.g. "$15,000"
    public string vendorOrTarget;
    public int requiredAuthLevel;
    public string signatureName;
}

[System.Serializable]
public class EvidenceData
{
    public string evidenceType; // "Receipt", "Access Log", "QA Report", "VIP Reference", "Leak Printout"
    public string[] lines;      // the visible text lines on the evidence paper
    public bool isTampered;
    public string tamperNote;   // internal note for eval, not shown to player
}

[System.Serializable]
public class CaseData
{
    public HRProfileData profile;
    public ActionProposalData proposal;
    public EvidenceData evidence;
    public bool isInnocent;
    public string villainType; // "Embezzler", "Kickback", "Nepotism", "TechRunner", "Saboteur"
    public string discrepancyHint; // dev/debug only
}

// ─── GENERATOR ─────────────────────────────────────────────────

public static class CaseGenerator
{
    private static readonly string[] FirstNames = { "철수","영희","지훈","민서","수현","현우","지원","서준","도윤","하은","유진","재범","소희","민수","태양","동현" };
    private static readonly string[] LastNames  = { "김","이","박","최","정","강","조","윤","장","임","한","오","서","신","권","황" };
    private static readonly string[] Departments = { "마케팅팀","개발팀","재무팀","인사팀","법무팀","운영팀","R&D연구소","영업팀" };
    private static readonly string[] Vendors     = { "에이펙스 상사","초록잎 물류","옴니텍 솔루션즈","푸른파도 미디어","크레스트 가구","노바파츠 주식회사","정상 케이터링","픽셀포지 스튜디오" };
    private static readonly string[] BlacklistedVendors = { "그림자 중개소","황금껍데기 무역","유령공작소" };

    private static readonly string[] InnocentTitles = { "사무용품 충전 구매","팀 회식비 정산","소프트웨어 라이선스 갱신","업무 출장비 청구","직무 교육 워크숍 참가비","사무기기 유지보수","바이어 접대비 정산","업무 서비스 정기구독" };
    private static readonly string[] VillainTitles   = { "임원 전용 비자금 계좌 접근","중앙 서버실 출입 승인","신작 게임 출시 최종 승인","대량 하드웨어 장비 구매","비상 예산 이체 요청","VIP 고객 선물 예산 청구","주말/야간 수당 청구","기밀 기획서 열람 요청" };

    public static CaseData Generate(int dayNumber)
    {
        // 50% innocent, 50% villain
        bool innocent = Random.value < 0.5f;

        if (innocent) return GenerateInnocent();

        // Pick villain type, weight by day for progression
        int villainIndex = PickVillain(dayNumber);
        return villainIndex switch
        {
            0 => GenerateEmbezzler(),
            1 => GenerateKickback(),
            2 => GenerateNepotism(),
            3 => GenerateTechRunner(),
            4 => GenerateSaboteur(),
            _ => GenerateInnocent()
        };
    }

    // ─── INNOCENT ──────────────────────────────────────────

    private static CaseData GenerateInnocent()
    {
        var profile = MakeProfile();
        int amount = Random.Range(50, 5000);
        string vendor = Vendors[Random.Range(0, Vendors.Length)];
        string title = InnocentTitles[Random.Range(0, InnocentTitles.Length)];

        var proposal = new ActionProposalData
        {
            date = GetRandomDate(),
            requestTitle = title,
            requestedAmount = $"${amount:N0}",
            vendorOrTarget = vendor,
            requiredAuthLevel = profile.securityLevel,
            signatureName = profile.fullName
        };

        var evidence = new EvidenceData
        {
            evidenceType = "영수증",
            isTampered = false,
            lines = new[]
            {
                $"영수증 - {vendor}",
                $"품목: {title}",
                $"총 금액: ${amount:N0}",
                $"일자: {proposal.date}",
                "상태: 결제 완료"
            }
        };

        return new CaseData
        {
            profile = profile, proposal = proposal, evidence = evidence,
            isInnocent = true, villainType = "None"
        };
    }

    // ─── VILLAIN 1: EMBEZZLER ──────────────────────────────

    private static CaseData GenerateEmbezzler()
    {
        var profile = MakeProfile();
        int realAmount = Random.Range(500, 3000);
        int fakeAmount = realAmount * 10; // extra zero
        string vendor = Vendors[Random.Range(0, Vendors.Length)];

        var proposal = new ActionProposalData
        {
            date = GetRandomDate(),
            requestTitle = "대량 하드웨어 장비 구매",
            requestedAmount = $"${fakeAmount:N0}",
            vendorOrTarget = vendor,
            requiredAuthLevel = profile.securityLevel,
            signatureName = profile.fullName
        };

        var evidence = new EvidenceData
        {
            evidenceType = "영수증",
            isTampered = true,
            tamperNote = $"기안서 금액(${fakeAmount:N0})과 영수증 금액(${realAmount:N0}) 불일치",
            lines = new[]
            {
                $"영수증 - {vendor}",
                "품목: 일반 사무용품",
                $"총 금액: ${realAmount:N0}",
                $"일자: {proposal.date}",
                "상태: 결제 완료"
            }
        };

        return new CaseData
        {
            profile = profile, proposal = proposal, evidence = evidence,
            isInnocent = false, villainType = "Embezzler",
            discrepancyHint = "기안서 금액과 영수증 금액 불일치."
        };
    }

    // ─── VILLAIN 2: KICKBACK ───────────────────────────────

    private static CaseData GenerateKickback()
    {
        var profile = MakeProfile();
        string badVendor = BlacklistedVendors[Random.Range(0, BlacklistedVendors.Length)];
        int amount = Random.Range(2000, 20000);

        var proposal = new ActionProposalData
        {
            date = GetRandomDate(),
            requestTitle = "사무기기 유지보수",
            requestedAmount = $"${amount:N0}",
            vendorOrTarget = badVendor,
            requiredAuthLevel = profile.securityLevel,
            signatureName = profile.fullName
        };

        var evidence = new EvidenceData
        {
            evidenceType = "영수증",
            isTampered = true,
            tamperNote = $"거래처 '{badVendor}'는 블랙리스트 대상임.",
            lines = new[]
            {
                $"영수증 - {badVendor}",
                "품목: 유지보수 서비스 계약",
                $"총 금액: ${amount:N0}",
                $"일자: {proposal.date}",
                "상태: 승인 대기"
            }
        };

        return new CaseData
        {
            profile = profile, proposal = proposal, evidence = evidence,
            isInnocent = false, villainType = "Kickback",
            discrepancyHint = "규정집의 블랙리스트 거래처 포함."
        };
    }

    // ─── VILLAIN 3: NEPOTISM ───────────────────────────────

    private static CaseData GenerateNepotism()
    {
        var profile = MakeProfile();
        profile.securityLevel = 1; // low-level intern

        string chairName = "김성식 회장";
        string misspelled = "김성싁 회장"; // typo
        bool useTypo = Random.value < 0.5f;
        string sigName = useTypo ? misspelled : chairName;
        string letterYear = useTypo ? "2026" : "2022"; // expired if not typo

        var proposal = new ActionProposalData
        {
            date = GetRandomDate(),
            requestTitle = "임원 전용 비자금 계좌 접근",
            requestedAmount = "$50,000",
            vendorOrTarget = "법인 자금 계정",
            requiredAuthLevel = 5, // requires Level 5
            signatureName = profile.fullName
        };

        var evidence = new EvidenceData
        {
            evidenceType = "VIP 추천서",
            isTampered = true,
            tamperNote = useTypo
                ? $"회장 성명 오탈자: '{sigName}'"
                : "2022년 발행 VIP 추천서 (기한 만료).",
            lines = new[]
            {
                "VIP 추천서",
                "관계자 귀하,",
                $"본인은 {profile.fullName}의 모든 접근 권한을 승인함.",
                $"서명: {sigName}",
                $"일자: {letterYear}년 3월 14일"
            }
        };

        return new CaseData
        {
            profile = profile, proposal = proposal, evidence = evidence,
            isInnocent = false, villainType = "Nepotism",
            discrepancyHint = "1등급 사원이 5등급 권한 요청. VIP 추천서 무효."
        };
    }

    // ─── VILLAIN 4: TECH RUNNER ────────────────────────────

    private static CaseData GenerateTechRunner()
    {
        var profile = MakeProfile();
        profile.department = "마케팅팀";
        string ip = $"192.168.{Random.Range(1,50)}.{Random.Range(2,254)}";
        profile.workstationIP = ip;

        int transferTB = Random.Range(100, 800);

        var proposal = new ActionProposalData
        {
            date = GetRandomDate(),
            requestTitle = "마케팅팀 업무 긴급 야근 청구",
            requestedAmount = "$2,400",
            vendorOrTarget = "내부 - 마케팅팀",
            requiredAuthLevel = profile.securityLevel,
            signatureName = profile.fullName
        };

        var evidence = new EvidenceData
        {
            evidenceType = "접근 기록",
            isTampered = true,
            tamperNote = $"중앙 서버실 출입 (마케팅팀 사원). 다운로드: {transferTB}TB.",
            lines = new[]
            {
                "중앙 서버 접근 기록",
                $"사용자: {profile.fullName} ({profile.employeeID})",
                $"부서: {profile.department}",
                "출입 장소: [중앙 서버실]",
                $"데이터 전송: 다운로드 {transferTB} TB",
                $"일시: {proposal.date} 23:47",
                $"단말기 IP: {ip}"
            }
        };

        return new CaseData
        {
            profile = profile, proposal = proposal, evidence = evidence,
            isInnocent = false, villainType = "TechRunner",
            discrepancyHint = "마케팅팀 사원이 중앙 서버실 접근 및 대용량 다운로드."
        };
    }

    // ─── VILLAIN 5: SABOTEUR ───────────────────────────────

    private static CaseData GenerateSaboteur()
    {
        var profile = MakeProfile();
        profile.department = "R&D연구소";
        string ip = $"10.0.{Random.Range(1,20)}.{Random.Range(2,254)}";
        profile.workstationIP = ip;

        bool useTamperedQA = Random.value < 0.5f;

        var proposal = new ActionProposalData
        {
            date = GetRandomDate(),
            requestTitle = "신작 게임 출시 최종 승인",
            requestedAmount = "해당 없음",
            vendorOrTarget = "내부 - R&D연구소",
            requiredAuthLevel = profile.securityLevel,
            signatureName = profile.fullName
        };

        EvidenceData evidence;

        if (useTamperedQA)
        {
            evidence = new EvidenceData
            {
                evidenceType = "QA 검수 보고서",
                isTampered = true,
                tamperNote = "QA 보고서의 불합격 표시가 취소선 처리되고 수기로 합격 기재됨.",
                lines = new[]
                {
                    "QA 최종 검수 보고서",
                    $"프로젝트: 피닉스 프로젝트 v2.1",
                    $"검수자: {profile.fullName}",
                    $"결과: ~~불합격(FAIL)~~ → 합격(PASS) [수기 작성]",
                    $"비고: '문제 없음, 그냥 출시 바람.'",
                    $"일자: {proposal.date}"
                }
            };
        }
        else
        {
            evidence = new EvidenceData
            {
                evidenceType = "유출 출력물",
                isTampered = true,
                tamperNote = $"유출 IP ({ip})가 사원 단말기 IP와 일치함.",
                lines = new[]
                {
                    "익명 유출 문건",
                    "제목: 미공개 빌드 v2.1",
                    "내용: [기밀 게임 에셋 유출]",
                    "유출 IP: " + ip,
                    $"게시일시: {proposal.date} 02:13",
                    "상태: 조사 진행 중"
                }
            };
        }

        return new CaseData
        {
            profile = profile, proposal = proposal, evidence = evidence,
            isInnocent = false, villainType = "Saboteur",
            discrepancyHint = useTamperedQA
                ? "QA 보고서 수기 변조됨."
                : "유출 IP가 사원 단말기 IP와 일치."
        };
    }

    // ─── HELPERS ───────────────────────────────────────────

    private static HRProfileData MakeProfile()
    {
        string first = FirstNames[Random.Range(0, FirstNames.Length)];
        string last  = LastNames[Random.Range(0, LastNames.Length)];
        return new HRProfileData
        {
            fullName    = $"{last}{first}",
            department  = Departments[Random.Range(0, Departments.Length)],
            employeeID  = $"EMP-{Random.Range(1000,9999)}",
            securityLevel = Random.Range(1, 6),
            workstationIP = $"192.168.{Random.Range(1,50)}.{Random.Range(2,254)}",
            photoSilhouetteID = $"silhouette_{Random.Range(1,7)}"
        };
    }

    private static string GetRandomDate()
    {
        int m = Random.Range(1, 13);
        int d = Random.Range(1, 29);
        return $"{m:D2}/{d:D2}/2026";
    }

    private static int PickVillain(int day)
    {
        // Day 1: villains 0-1, Day 2: 0-2, Day 3+: all 5
        int maxIndex = Mathf.Min(day + 1, 5);
        return Random.Range(0, maxIndex);
    }
}